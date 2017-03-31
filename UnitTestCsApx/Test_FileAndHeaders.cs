using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using RemoteFile;
using Apx;


namespace UnitTestCsApx
{
    [TestClass]
    public class Test_FileAndHeaders
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
        public void initFile()
        {
            RemoteFile.File testFile = new RemoteFile.File();

        }


        /*[TestMethod]
        public void TEMPtestFiles()
        {
            Apx.File testFile;
            Apx.FileMap testMap = new Apx.FileMap();

            testFile = new Apx.File(); testFile.address = 0;
            testMap.sortedAddFileToList(testFile);
            testFile = new Apx.File(); testFile.address = 0x50;
            testMap.sortedAddFileToList(testFile);
            testFile = new Apx.File(); testFile.address = 0x20;
            testMap.sortedAddFileToList(testFile);
            
            testMap.assignFileAddress(testFile, 0, 2, 2);
        }
        */

        [TestMethod]
        public void testFileMapOrder1()
        {
            Apx.File f1, f2, f3, f4, f5, f6, f7, f8;
            Apx.FileMap testMap = new Apx.FileMap();

            f1 = new Apx.File(); f1.name = "test1.out"; f1.length = 6840;
            testMap.assignFileAddressDefault(f1);
            f2 = new Apx.File(); f2.name = "test2.in"; f2.length = 64;
            testMap.assignFileAddressDefault(f2);
            f3 = new Apx.File(); f3.name = "test3.apx"; f3.length = 64;
            testMap.assignFileAddressDefault(f3);
            f4 = new Apx.File(); f4.name = "test4.apx"; f4.length = 100;
            testMap.assignFileAddressDefault(f4);
            f5 = new Apx.File(); f5.name = "test5.out"; f5.length = 8000;
            testMap.assignFileAddressDefault(f5);
            f6 = new Apx.File(); f6.name = "test6.in"; f6.length = 400;
            testMap.assignFileAddressDefault(f6);
            f7 = new Apx.File(); f7.name = "test7.bin"; f7.length = 6200;
            testMap.assignFileAddressDefault(f7);
            f8 = new Apx.File(); f8.name = "test8.png"; f8.length = 1234;
            testMap.assignFileAddressDefault(f8);

            Assert.AreEqual(testMap._items.Count, 8);
            Assert.AreEqual(testMap._items[0], f1);
            Assert.AreEqual(testMap._items[1], f2);
            Assert.AreEqual(testMap._items[2], f5);
            Assert.AreEqual(testMap._items[3], f6);
            Assert.AreEqual(testMap._items[4], f3);
            Assert.AreEqual(testMap._items[5], f4);
            Assert.AreEqual(testMap._items[6], f7);
            Assert.AreEqual(testMap._items[7], f8);
        }

        [TestMethod]
        public void testFileMapOrder2()
        {
            Apx.File f1, f2, f3, f4, f5, f6, f7, f8;
            Apx.FileMap testMap = new Apx.FileMap();

            f4 = new Apx.File(); f4.name = "test4.apx"; f4.length = 100;
            testMap.assignFileAddressDefault(f4);
            f1 = new Apx.File(); f1.name = "test1.out"; f1.length = 6840;
            testMap.assignFileAddressDefault(f1);
            f7 = new Apx.File(); f7.name = "test7.bin"; f7.length = 6200;
            testMap.assignFileAddressDefault(f7);
            f2 = new Apx.File(); f2.name = "test2.in"; f2.length = 64;
            testMap.assignFileAddressDefault(f2);
            f3 = new Apx.File(); f3.name = "test3.apx"; f3.length = 64;
            testMap.assignFileAddressDefault(f3);
            f5 = new Apx.File(); f5.name = "test5.out"; f5.length = 8000;
            testMap.assignFileAddressDefault(f5);
            f6 = new Apx.File(); f6.name = "test6.in"; f6.length = 400;
            testMap.assignFileAddressDefault(f6);
            f8 = new Apx.File(); f8.name = "test8.png"; f8.length = 1234;
            testMap.assignFileAddressDefault(f8);

            List<ulong> addresses = testMap.getAddressList();
            List<string> names = testMap.getNameList();

            Assert.AreEqual(testMap._items.Count, 8);
            Assert.AreEqual(testMap._items[0], f1);
            Assert.AreEqual(testMap._items[1], f2);
            Assert.AreEqual(testMap._items[2], f5);
            Assert.AreEqual(testMap._items[3], f6);
            Assert.AreEqual(testMap._items[4], f4);
            Assert.AreEqual(testMap._items[5], f3);
            Assert.AreEqual(testMap._items[6], f7);
            Assert.AreEqual(testMap._items[7], f8);
        }

        [TestMethod]
        public void testFileMapAreaFull()
        {
            Apx.File f1, f2, f3, f4, f5, f6, f7, f8;
            Apx.FileMap testMap = new Apx.FileMap();
            bool res = false;

            f1 = new Apx.File(); f1.name = "test1.out"; f1.length = 0x3F00000;
            res = testMap.assignFileAddressDefault(f1);
            Assert.AreEqual(res, true);
            f2 = new Apx.File(); f2.name = "test2.out"; f2.length = 0xF0000;
            res = testMap.assignFileAddressDefault(f2);
            Assert.AreEqual(res, true);
            f3 = new Apx.File(); f3.name = "test3.out"; f3.length = 100;
            res = testMap.assignFileAddressDefault(f3);
            Assert.AreEqual(res, true);
            f4 = new Apx.File(); f4.name = "test4.out"; f4.length = 0x500000; // Should not be added
            res = testMap.assignFileAddressDefault(f4);
            Assert.AreEqual(res, false);
            f5 = new Apx.File(); f5.name = "test5.out"; f5.length = 0x495000; // Should not be added
            res = testMap.assignFileAddressDefault(f5);
            Assert.AreEqual(res, false);
            f6 = new Apx.File(); f6.name = "test6.apx"; f6.length = 0x95000; // New area, should be added
            res = testMap.assignFileAddressDefault(f6);
            Assert.AreEqual(res, true);
            f7 = new Apx.File(); f7.name = "test7.out"; f7.length = 100;
            res = testMap.assignFileAddressDefault(f7);
            Assert.AreEqual(res, true);
            f8 = new Apx.File(); f8.name = "test8.out"; f8.length = 1234;
            res = testMap.assignFileAddressDefault(f8);
            Assert.AreEqual(res, true);

            List<ulong> addresses = testMap.getAddressList();
            List<string> names = testMap.getNameList();

            Assert.AreEqual(testMap._items.Count, 6);
            Assert.AreEqual(testMap._items[0], f1);
            Assert.AreEqual(testMap._items[1], f2);
            Assert.AreEqual(testMap._items[2], f3);
            Assert.AreEqual(testMap._items[3], f7);
            Assert.AreEqual(testMap._items[4], f8);
            Assert.AreEqual(testMap._items[5], f6);
        }

    }
}
