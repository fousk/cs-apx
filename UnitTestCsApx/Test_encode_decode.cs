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
    public class Test_encode_decode
    {
        [TestMethod]
        public void encode_32()
        {
            byte[] res = NumHeader.pack32(0x00);
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0x00 }));
            res = NumHeader.pack32(127);
            //8b
            res = NumHeader.encode(0, 32).ToArray();
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0x00 }));
            res = NumHeader.encode(0x7F, 32).ToArray();
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0x7F }));
            // 32b
            /*
            32767	"\xFF\FF"	"\x80\x00\x7F\xFF"
            32768	"\x80\00"	"\x80\x00\x80\x00"
            32895	"\x80\7F"	"\x80\x00\x80\x7F"
            */
            res = NumHeader.encode(0x80, 32).ToArray();
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0x80, 0x00, 0x00, 0x80 }));
            res = NumHeader.encode(0x7FFF, 32).ToArray();
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0x80, 0x00, 0x7F, 0xFF }));
            res = NumHeader.encode(0x8000, 32).ToArray();
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0x80, 0x00, 0x80, 0x00}));
            res = NumHeader.encode(0x807F, 32).ToArray();
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0x80, 0x00, 0x80, 0x7F}));
            res = NumHeader.encode(0x7FFFFFFF, 32).ToArray();
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }));
        }

        [TestMethod]
        public void decode_32()
        {
            NumHeader.decodeReturn ret = NumHeader.decode(new List<byte> { 0x00 },0, 32);
            Assert.AreEqual(ret.value, (uint)0x00);
            Assert.AreEqual(ret.bytesParsed, 1);
            ret = NumHeader.decode(new List<byte> { 0x7F },0, 32);
            Assert.AreEqual(ret.value, (uint)0x7F);
            Assert.AreEqual(ret.bytesParsed, 1);
            ret = NumHeader.decode(new List<byte> { 0x80, 0x00, 0x00, 0x80 },0, 32);
            Assert.AreEqual(ret.value, (uint)0x80);
            Assert.AreEqual(ret.bytesParsed, 4);
            ret = NumHeader.decode(new List<byte> { 0x80, 0x00, 0x7F, 0xFF },0, 32);
            Assert.AreEqual(ret.value, (uint)0x7FFF);
            Assert.AreEqual(ret.bytesParsed, 4);
            ret = NumHeader.decode(new List<byte> { 0x80, 0x00, 0x80, 0x00 },0, 32);
            Assert.AreEqual(ret.value, (uint)0x8000);
            Assert.AreEqual(ret.bytesParsed, 4);
            ret = NumHeader.decode(new List<byte> { 0xFF, 0xFF, 0xFF, 0xFF },0, 32);
            Assert.AreEqual(ret.value, (uint)0x7FFFFFFF);
            Assert.AreEqual(ret.bytesParsed, 4);
        }
    }
}
