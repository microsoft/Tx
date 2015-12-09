# Reading network captures

In these samples we illustrate how to read network captures, from files file formats supported by [Wireshark](http://www.wireshark.org/).

The samples are based on packets exported from the Wireshark [sample for SNMP](https://wiki.wireshark.org/SampleCaptures#SNMP) in [.pcapng](https://www.winpcap.org/ntar/draft/PCAP-DumpFileFormat.html) format. See the complete code in [Program.cs](Program.cs).

## Pcap next generation reading
The pcapng files contain "blocks" of many types, such as header section, interface description, as well as captured packets. 

Here is how to read the first 5 blocks at that level:

    foreach (var block in PcapNg.ReadForward(fileName).Take(5))
        Console.WriteLine("{0} {1}", block.Length, block.Type);

The file format supports reading forward and backward, and we only implemented forward reading so far.

Usually the most interesting data are the captured packets:

    var packets = PcapNg.ReadForward(fileName)
        .Where(b => b.Type == BlockType.EnhancedPacketBlock)
        .Cast<EnhancedPacketBlock>().Take(5);

    foreach (var packet in packets)
        Console.WriteLine("{0} {1} {2}", packet.TimestampUtc, packet.PacketLen, packet.CapturedLen);

Here PacketLen is the original length, and CapturedLen is the subset that was captured.

If you want to get better feel how the data looks like, you can use [LINQPad](http://linqpad.net):
- Press F4, and add the NuGet package Tx.Network
- Add the namespace Tx.Network that comes with it
- Type this in the query window:

	  PcapNg.ReadForward(@"c:\git\tx\traces\snmp.pcapng").Take(5)

- Run the query

## Decoding up to UDP protocol level
To decode the same packets as UDP datagrams we can do this:

    foreach (var packet in packets)
    {
        int ipLen = packet.PacketData.Length - 14; // 14 is the size of the Ethernet header
        byte[] datagram = new byte[ipLen];
        Array.Copy(packet.PacketData, 14, datagram, 0, ipLen);

        UdpDatagram udp = new UdpDatagram(datagram);
        Console.WriteLine(udp.PacketData.ToHexDump());
        Console.WriteLine();
    }

Here the concept of [UdpDatagram](../../../Source/Tx.Network/UDP.cs) comes from Tx.Network and [ToHexDump()](../../../Source/Tx.Core/ByteArrayExtensions.cs) comes from Tx.Core. As you can see we have not tried to interpret the payload so far.

## Decoding as SNMP

SNMP (Simple Network Management Protocol) datagrams can be interpreted using two layers:
- The data is encoded in Abstract Syntax Notation 1 (ASN1), using "Basic Encoding Rules"
- Assuming this as basis, SNMP defines "Protocol Data Units" (PDU) such as Get, Get-Response and Trap

### Single-pass read using Basic Encoding Rules

To read the data from the ASN1 format, without further interpretation:

    foreach (var packet in packets)
    {
        int snmpLen = packet.PacketData.Length - 42; // 42 is the size of Ethernet + IP + UDP headers
        byte[] datagram = new byte[snmpLen];
        Array.Copy(packet.PacketData, 42, datagram, 0, snmpLen);
        Console.WriteLine(BasicEncodingReader.ReadAllText(datagram));
    }
 
The ReadAllText() method formats the content in a tree similar to WireShark. 

To use the data conveniently without learning ASN1 or parsing this test, one should use the reader methods to read types, length and values of the fields instead.

### Decoding Protocol Data Units (PDU-s)

In this level we read the entire datagram into in-memory structure:

    var snmp = SnmpCapture.ReadPcapNg(fileName)
        .Take(5);

    foreach (var pdu in snmp)
        Console.WriteLine(pdu.ToString());

Here SnmpCapture is convenience reader class that assumes the files contain only SNMP packets. 

You can set a break point in the last line to see the structure. The most interesting data is usually in the VarBinds collection. 

Alternatively, you can see the data formatted better by using [LINQPad](http://linqpad.net):
- Press F4, and add the NuGet package Tx.Network
- Add the namespaces Tx.Network and Tx.Network.Snmp from the package
- Type this in the query window:

		SnmpCapture.ReadPcapNg(@"c:\git\tx\traces\snmp.pcapng")

- Run the query