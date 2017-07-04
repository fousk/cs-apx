using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//using MiscUtil.Conversion;
using System.Linq;
using RemoteFile;
using Apx;

namespace UnitTestCsApx
{
    [TestClass]
    public class Test_SignalDecoding
    {
        [TestMethod]
        public void Test_TypeToReadable_NoArray()
        {
            NodeData nd = new NodeData();
            string txt;
            txt = nd.typeToReadable('a', 2, new byte[] { 0x41, 0x42 });
            Assert.AreEqual("\"AB\"", txt);
            // 8-bit
            txt = nd.typeToReadable('c', 1, new byte[] { 0x10 });
            Assert.AreEqual("16", txt);
            txt = nd.typeToReadable('c', 1, new byte[] { 0xFF });
            Assert.AreEqual("-1", txt);
            txt = nd.typeToReadable('C', 1, new byte[] { 0xFF });
            Assert.AreEqual("255", txt);
            // 16-bit
            txt = nd.typeToReadable('s', 1, new byte[] { 0x10, 0x10 });
            Assert.AreEqual("4112", txt);
            txt = nd.typeToReadable('s', 1, new byte[] { 0xFB, 0xFF });
            Assert.AreEqual("-5", txt);
            txt = nd.typeToReadable('S', 1, new byte[] { 0x10, 0x10 });
            Assert.AreEqual("4112", txt);
            txt = nd.typeToReadable('S', 1, new byte[] { 0xFB, 0xFF });
            Assert.AreEqual("65531", txt);
            // 32-bit
            txt = nd.typeToReadable('l', 1, new byte[] { 0x13, 0x12, 0x11, 0x10 });
            Assert.AreEqual("269554195", txt);
            txt = nd.typeToReadable('l', 1, new byte[] { 0x79, 0xF5, 0xFF, 0xFF });
            Assert.AreEqual("-2695", txt);
            txt = nd.typeToReadable('L', 1, new byte[] { 0x13, 0x12, 0x11, 0x10 });
            Assert.AreEqual("269554195", txt);
            txt = nd.typeToReadable('L', 1, new byte[] { 0x79, 0xF5, 0xFF, 0xFF });
            Assert.AreEqual("4294964601", txt);
            // 64 bit
            txt = nd.typeToReadable('u', 1, new byte[] { 0x13, 0x12, 0x11, 0x10, 0x00, 0x00, 0x00, 0x00 });
            Assert.AreEqual("269554195", txt);
            txt = nd.typeToReadable('u', 1, new byte[] { 0x79, 0xF5, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            Assert.AreEqual("-2695", txt);
            txt = nd.typeToReadable('U', 1, new byte[] { 0x13, 0x12, 0x11, 0x10, 0x00, 0x00, 0x00, 0x00 });
            Assert.AreEqual("269554195", txt);
            txt = nd.typeToReadable('U', 1, new byte[] { 0x79, 0xF5, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            Assert.AreEqual("18446744073709548921", txt);
        }
    }
}
