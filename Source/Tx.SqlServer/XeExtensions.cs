// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reactive;

namespace Tx.SqlServer
{
    public static class XelExtensions
    {
        [FileParser("XEvent", ".xel")]
        public static void AddXelFiles(this IPlaybackConfiguration playback, params string[] xeFiles)
        {
            playback.AddInput(
                () => XeObservable.FromFiles(xeFiles),
                typeof (XeTypeMap));
        }

        public static void AddXeTarget<TTarget>(this IPlaybackConfiguration playback) where TTarget : XeSubject, new()
        {
            playback.AddInput(
                () => XeObservable.FromTarget<TTarget>(),
                typeof (XeTypeMap));
        }
    }
}