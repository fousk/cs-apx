using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using RemoteFile;
using Apx;
//using MiscUtil.Conversion;

namespace UnitTestCsApx
{
    [TestClass]
    public class Test_Pack_UnPack
    {
        
        [TestMethod]
        public void TestPack32()
        {
            /* Example table from documentation
             * Value	    NumHeader16	NumHeader32
                0	        "\x00"	    "\x00"
                127	        "\x7F"	    "\x7F"
                128	        "\x80\x80"	"\x80\x00\x00\x80"
                32767   	"\xFF\FF"	"\x80\x00\x7F\xFF"
                32768	    "\x80\00"	"\x80\x00\x80\x00"
                32895	    "\x80\7F"	"\x80\x00\x80\x7F"
                2147483647	-	        "\xFF\xFF\xFF\xFF"
             */
            byte[] res = NumHeader.pack32(0x00);
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0x00 }));
            res = NumHeader.pack32(127);
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0x7F }));
            res = NumHeader.pack32(128);
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0x80, 0x00, 0x00, 0x80 }));
            res = NumHeader.pack32(32767);
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0x80, 0x00, 0x7F, 0xFF }));
            res = NumHeader.pack32(32768);
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0x80, 0x00, 0x80, 0x00 }));
            res = NumHeader.pack32(32895);
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0x80, 0x00, 0x80, 0x7F }));
            res = NumHeader.pack32(2147483647);
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }));
        }

        [TestMethod]
        public void TestUnPack32()
        {
            /* Example table from documentation
             * Value	    NumHeader16	NumHeader32
                0	        "\x00"	    "\x00"
                127	        "\x7F"	    "\x7F"
                128	        "\x80\x80"	"\x80\x00\x00\x80"
                32767   	"\xFF\FF"	"\x80\x00\x7F\xFF"
                32768	    "\x80\00"	"\x80\x00\x80\x00"
                32895	    "\x80\7F"	"\x80\x00\x80\x7F"
                2147483647	-	        "\xFF\xFF\xFF\xFF"
             */
            ulong res = NumHeader.unpack32(new byte[] { 0x00 });
            Assert.IsTrue(res == 0);
            res = NumHeader.unpack32(new byte[] { 0x7F });
            Assert.IsTrue(res == 127);
            res = NumHeader.unpack32(new byte[] { 0x80, 0x00, 0x00, 0x80 });
            Assert.IsTrue(res == 128);
            res = NumHeader.unpack32(new byte[] { 0x80, 0x00, 0x7F, 0xFF });
            Assert.IsTrue(res == 32767);
            res = NumHeader.unpack32(new byte[] { 0x80, 0x00, 0x80, 0x00 });
            Assert.IsTrue(res == 32768);
            res = NumHeader.unpack32(new byte[] { 0x80, 0x00, 0x80, 0x7F });
            Assert.IsTrue(res == 32895);
            res = NumHeader.unpack32(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
            Assert.IsTrue(res == 2147483647);
        }

        [TestMethod]
        public void packHeader()
        {
            byte[] res = RemoteFileUtil.packHeader(0, false);
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0x00, 0x00 }));
            res = RemoteFileUtil.packHeader(0, true);
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0x40, 0x00 }));
            res = RemoteFileUtil.packHeader(1234, true);
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0x44, 0xD2 }));
            res = RemoteFileUtil.packHeader(16383, false);
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0x3F, 0xFF }));
            res = RemoteFileUtil.packHeader(16383, true);
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0x7F, 0xFF }));
            res = RemoteFileUtil.packHeader(16384, false);
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0x80, 0x00, 0x40, 0x00 }));
            res = RemoteFileUtil.packHeader(16384, true);
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0xC0, 0x00, 0x40, 0x00 }));
            res = RemoteFileUtil.packHeader(1073741823, false);
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0xBF, 0xFF, 0xFF, 0xFF }));
            res = RemoteFileUtil.packHeader(1073741823, true);
            Assert.IsTrue(res.SequenceEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }));
        }

        [TestMethod]
        public void unpackHeader()
        {
            // Valid data
            RemoteFileUtil.headerReturn res = RemoteFileUtil.unpackHeader(new byte[] { 0x00, 0x00 });
            Assert.IsTrue((res.more_bit == false) && (res.address == 0) && (res.bytes_parsed == 2));
            res = RemoteFileUtil.unpackHeader(new byte[] { 0x40, 0x00 });
            Assert.IsTrue((res.more_bit == true) && (res.address == 0) && (res.bytes_parsed == 2));
            res = RemoteFileUtil.unpackHeader(new byte[] { 0x44, 0xD2 });
            Assert.IsTrue((res.more_bit == true) && (res.address == 1234) && (res.bytes_parsed == 2));
            res = RemoteFileUtil.unpackHeader(new byte[] { 0x3F, 0xFF });
            Assert.IsTrue((res.more_bit == false) && (res.address == 16383) && (res.bytes_parsed == 2));
            res = RemoteFileUtil.unpackHeader(new byte[] { 0x7F, 0xFF });
            Assert.IsTrue((res.more_bit == true) && (res.address == 16383) && (res.bytes_parsed == 2));
            res = RemoteFileUtil.unpackHeader(new byte[] { 0x80, 0x00, 0x40, 0x00 });
            Assert.IsTrue((res.more_bit == false) && (res.address == 16384) && (res.bytes_parsed == 4));
            res = RemoteFileUtil.unpackHeader(new byte[] { 0xC0, 0x00, 0x40, 0x00 });
            Assert.IsTrue((res.more_bit == true) && (res.address == 16384) && (res.bytes_parsed == 4));
            res = RemoteFileUtil.unpackHeader(new byte[] { 0xBF, 0xFF, 0xFF, 0xFF });
            Assert.IsTrue((res.more_bit == false) && (res.address == 1073741823) && (res.bytes_parsed == 4));
            res = RemoteFileUtil.unpackHeader(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
            Assert.IsTrue((res.more_bit == true) && (res.address == 1073741823) && (res.bytes_parsed == 4));

            // Longer data fields
            res = RemoteFileUtil.unpackHeader(new byte[] { 0x00, 0x00, 0x00 });
            Assert.IsTrue((res.more_bit == false) && (res.address == 0) && (res.bytes_parsed == 2));
            res = RemoteFileUtil.unpackHeader(new byte[] { 0x00, 0x00, 0x00, 0x00});
            Assert.IsTrue((res.more_bit == false) && (res.address == 0) && (res.bytes_parsed == 2));
            res = RemoteFileUtil.unpackHeader(new byte[] { 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            Assert.IsTrue((res.more_bit == true) && (res.address == 16383) && (res.bytes_parsed == 2));
            res = RemoteFileUtil.unpackHeader(new byte[] { 0xBF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            Assert.IsTrue((res.more_bit == false) && (res.address == 1073741823) && (res.bytes_parsed == 4));
            res = RemoteFileUtil.unpackHeader(new byte[] { 0xBF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            Assert.IsTrue((res.more_bit == false) && (res.address == 1073741823) && (res.bytes_parsed == 4));

            // Too short data fields
            res = RemoteFileUtil.unpackHeader(new byte[] { 0x00});
            Assert.IsTrue((res.more_bit == false) && (res.address == ulong.MaxValue) && (res.bytes_parsed == 0));
            res = RemoteFileUtil.unpackHeader(new byte[] { 0x4E });
            Assert.IsTrue((res.more_bit == false) && (res.address == ulong.MaxValue) && (res.bytes_parsed == 0));
            res = RemoteFileUtil.unpackHeader(new byte[] { 0xC0});
            Assert.IsTrue((res.more_bit == false) && (res.address == ulong.MaxValue) && (res.bytes_parsed == 0));
            res = RemoteFileUtil.unpackHeader(new byte[] { 0xC0, 0x00});
            Assert.IsTrue((res.more_bit == false) && (res.address == ulong.MaxValue) && (res.bytes_parsed == 0));
            res = RemoteFileUtil.unpackHeader(new byte[] { 0x80, 0x00, 0x40});
            Assert.IsTrue((res.more_bit == false) && (res.address == ulong.MaxValue) && (res.bytes_parsed == 0));
            res = RemoteFileUtil.unpackHeader(new byte[] { 0x80, 0x00 });
            Assert.IsTrue((res.more_bit == false) && (res.address == ulong.MaxValue) && (res.bytes_parsed == 0));
        }

        
        [TestMethod]
        public void test_packFileOpen()
        {
            List<byte> blist = new List<byte>();
            blist = RemoteFileUtil.packFileOpen(0x12345678, "<");
            // Address
            Assert.AreEqual(blist[7], (byte)0x12);
            Assert.AreEqual(blist[6], (byte)0x34);
            Assert.AreEqual(blist[5], (byte)0x56);
            Assert.AreEqual(blist[4], (byte)0x78);
            // Command (RMF_CMD_FILE_OPEN == 10 / 0x0A)
            Assert.AreEqual(blist[3], (byte)0x00);
            Assert.AreEqual(blist[2], (byte)0x00);
            Assert.AreEqual(blist[1], (byte)0x00);
            Assert.AreEqual(blist[0], (byte)0x0A);
        }

        [TestMethod]
        public void test_unPackFileOpen()
        {
            List<byte> blist = new List<byte>{0x0A, 0x00, 0x00, 0x00, 0x78, 0x56, 0x34, 0x12};
            uint address = RemoteFileUtil.unPackFileOpen(blist, "<");
            Assert.AreEqual(address, (uint)0x12345678);

            try
            {
                List<byte> blist2 = new List<byte> { 0x0B, 0x00, 0x00, 0x00, 0x78, 0x56, 0x34, 0x12 };
                address = RemoteFileUtil.unPackFileOpen(blist2, "<");
                Assert.Fail("An exception should have been thrown");
            }
            catch (ArgumentException ae)
            {
                Assert.AreEqual(true, (ae.Message.Length > 0));
            }
            catch (Exception e)
            {
                Assert.Fail(
                     string.Format("Unexpected exception of type {0} caught: {1}",
                                    e.GetType(), e.Message)
                );
            }
        }

        [TestMethod]
        public void test_packFileClose()
        {
            List<byte> blist = new List<byte>();
            blist = RemoteFileUtil.packFileClose(0x12345678, "<");
            // Address
            Assert.AreEqual(blist[7], (byte)0x12);
            Assert.AreEqual(blist[6], (byte)0x34);
            Assert.AreEqual(blist[5], (byte)0x56);
            Assert.AreEqual(blist[4], (byte)0x78);
            // Command (RMF_CMD_FILE_CLOSE == 11 / 0x0B)
            Assert.AreEqual(blist[3], (byte)0x00);
            Assert.AreEqual(blist[2], (byte)0x00);
            Assert.AreEqual(blist[1], (byte)0x00);
            Assert.AreEqual(blist[0], (byte)0x0B);
        }

        [TestMethod]
        public void test_unPackFileClose()
        {
            List<byte> blist = new List<byte> { 0x0B, 0x00, 0x00, 0x00, 0x78, 0x56, 0x34, 0x12 };
            uint address = RemoteFileUtil.unPackFileClose(blist, "<");
            Assert.AreEqual(address, (uint)0x12345678);

            try
            {
                List<byte> blist2 = new List<byte> { 0x0A, 0x00, 0x00, 0x00, 0x78, 0x56, 0x34, 0x12 };
                address = RemoteFileUtil.unPackFileClose(blist2, "<");
                Assert.Fail("An exception should have been thrown");
            }
            catch (ArgumentException ae)
            {
                Assert.AreEqual(true, (ae.Message.Length > 0));
            }
            catch (Exception e)
            {
                Assert.Fail(
                     string.Format("Unexpected exception of type {0} caught: {1}",
                                    e.GetType(), e.Message)
                );
            }
        }

        [TestMethod]
        public void test_packFileInfo()
        {
            byte[] test = new byte[] {0x03, 0x00, 0x00, 0x00};
            RemoteFile.File file1 = new RemoteFile.File();
            file1.name = "test.txt";
            file1.length = 100;
            file1.address = 10000;
            List<byte> data = RemoteFileUtil.packFileInfo(file1, "<");
            Assert.AreEqual(data.Count, RemoteFile.Constants.RMF_FILEINFO_BASE_LEN + file1.name.Length + 1); // +1 null termination
            Assert.IsTrue(data.GetRange(0, 4).SequenceEqual(new List<byte> { 0x03, 0x00, 0x00, 0x00 }));
            Assert.IsTrue(data.GetRange(4, 4).SequenceEqual(new List<byte> { 0x10, 0x27, 0x00, 0x00 }));
            Assert.IsTrue(data.GetRange(8, 4).SequenceEqual(new List<byte> { 0x64, 0x00, 0x00, 0x00 }));
            Assert.IsTrue(data.GetRange(12, 4).SequenceEqual(new List<byte> { 0x00, 0x00, 0x00, 0x00 }));
            foreach(byte b in data.GetRange(16, 32))
            { Assert.IsTrue(b == 0); }
            byte[] bname = data.GetRange(48, data.Count - 48 -1).ToArray(); // -1 for null termination
            string fileName = System.Text.Encoding.ASCII.GetString(bname);
            Assert.AreEqual("test.txt", fileName);
            Assert.IsTrue(data[data.Count -1] == 0);
        }

        [TestMethod]
        public void test_unPackFileInfo()
        {
            // Header content
            List<byte> data = new List<byte> {0x03, 0x00, 0x00, 0x00, 0x10, 0x27, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
            // Digest data
            List<byte> ddata = Enumerable.Repeat((byte)0, 32).ToList();
            data.AddRange(ddata);
            // Name
            List<byte> ndata = ASCIIEncoding.ASCII.GetBytes("test.txt").ToList();
            data.AddRange(ndata);
            // Null termination
            data.Add(0);
            RemoteFile.File file = RemoteFileUtil.unPackFileInfo(data, "<");

            Assert.AreEqual(file.address, (uint)10000);
            Assert.IsTrue(file.digestData.SequenceEqual(ddata));
            Assert.AreEqual(file.digestType, RemoteFile.Constants.RMF_DIGEST_TYPE_NONE);
            Assert.AreEqual(file.name, "test.txt");
        }

    }
}
