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
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class Test_FileMap
    {
        [TestMethod]
        public void Test_FileMapOrder1()
        {
            Apx.File f1, f2, f3, f4, f5, f6, f7, f8;
            Apx.FileMap testMap = new Apx.FileMap();
            bool res = false;

            f1 = new Apx.File("test1.out", 6840);
            res = testMap.assignFileAddressDefault(f1);
            testMap.insert(f1);
            f2 = new Apx.File("test2.in", 64);
            res = testMap.assignFileAddressDefault(f2);
            testMap.insert(f2);
            f3 = new Apx.File("test3.apx", 64);
            res = testMap.assignFileAddressDefault(f3);
            testMap.insert(f3);
            f4 = new Apx.File("test4.apx", 100);
            res = testMap.assignFileAddressDefault(f4);
            testMap.insert(f4);
            f5 = new Apx.File("test5.out", 8000);
            res = testMap.assignFileAddressDefault(f5);
            testMap.insert(f5);
            f6 = new Apx.File("test6.in", 400);
            res = testMap.assignFileAddressDefault(f6);
            testMap.insert(f6);
            f7 = new Apx.File("test7.bin", 6200);
            res = testMap.assignFileAddressDefault(f7);
            testMap.insert(f7);
            f8 = new Apx.File("test8.png", 1234);
            res = testMap.assignFileAddressDefault(f8);
            testMap.insert(f8);

            Assert.AreEqual(testMap.fileList.Count, 8);
            Assert.AreEqual(testMap.fileList[0], f1);
            Assert.AreEqual(testMap.fileList[1], f2);
            Assert.AreEqual(testMap.fileList[2], f5);
            Assert.AreEqual(testMap.fileList[3], f6);
            Assert.AreEqual(testMap.fileList[4], f3);
            Assert.AreEqual(testMap.fileList[5], f4);
            Assert.AreEqual(testMap.fileList[6], f7);
            Assert.AreEqual(testMap.fileList[7], f8);
        }

        [TestMethod]
        public void Test_FileMapOrder2()
        {
            Apx.File f1, f2, f3, f4, f5, f6, f7, f8;
            Apx.FileMap testMap = new Apx.FileMap();
            bool res = false;

            f4 = new Apx.File("test4.apx", 100);
            res = testMap.assignFileAddressDefault(f4);
            testMap.insert(f4);
            f1 = new Apx.File("test1.out", 6840);
            testMap.assignFileAddressDefault(f1);
            testMap.insert(f1);
            f7 = new Apx.File("test7.bin", 6200);
            testMap.assignFileAddressDefault(f7);
            testMap.insert(f7);
            f2 = new Apx.File("test2.in", 64);
            testMap.assignFileAddressDefault(f2);
            testMap.insert(f2);
            f3 = new Apx.File("test3.apx", 64);
            testMap.assignFileAddressDefault(f3);
            testMap.insert(f3);
            f5 = new Apx.File("test5.out", 8000);
            testMap.assignFileAddressDefault(f5);
            testMap.insert(f5);
            f6 = new Apx.File("test6.in", 400);
            testMap.assignFileAddressDefault(f6);
            testMap.insert(f6);
            f8 = new Apx.File("test8.png", 1234);
            testMap.assignFileAddressDefault(f8);
            testMap.insert(f8);

            List<uint> addresses = testMap.getAddressList();
            List<string> names = testMap.getNameList();

            Assert.AreEqual(testMap.fileList.Count, 8);
            Assert.AreEqual(testMap.fileList[0], f1);
            Assert.AreEqual(testMap.fileList[1], f2);
            Assert.AreEqual(testMap.fileList[2], f5);
            Assert.AreEqual(testMap.fileList[3], f6);
            Assert.AreEqual(testMap.fileList[4], f4);
            Assert.AreEqual(testMap.fileList[5], f3);
            Assert.AreEqual(testMap.fileList[6], f7);
            Assert.AreEqual(testMap.fileList[7], f8);
        }

        [TestMethod]
        public void Test_FileMapAreaFull()
        {
            
            Apx.File f1, f2, f3, f4, f5, f6, f7, f8;
            Apx.FileMap testMap = new Apx.FileMap();
            bool res = false;

            f1 = new Apx.File("test1.out", 0x3F00000);
            res = testMap.assignFileAddressDefault(f1);
            Assert.AreEqual(res, true);
            res = testMap.insert(f1);
            Assert.AreEqual(res, true);
            f2 = new Apx.File("test2.out", 0xF0000);
            res = testMap.assignFileAddressDefault(f2); 
            Assert.AreEqual(res, true);
            res = testMap.insert(f2);
            Assert.AreEqual(res, true);
            f3 = new Apx.File("test3.out", 100);
            res = testMap.assignFileAddressDefault(f3);
            Assert.AreEqual(res, true);
            res = testMap.insert(f3);
            Assert.AreEqual(res, true);
            f4 = new Apx.File("test4.out", 0x500000); // Should not be added
            res = testMap.assignFileAddressDefault(f4);
            Assert.AreEqual(res, false);
            res = testMap.insert(f4);
            Assert.AreEqual(res, false);
            f5 = new Apx.File("test5.out", 0x495000); // Should not be added
            Assert.AreEqual(res, false);
            res = testMap.assignFileAddressDefault(f5);
            Assert.AreEqual(res, false);
            res = testMap.insert(f5);
            Assert.AreEqual(res, false);
            f6 = new Apx.File("test6.apx", 0x95000); // New area, should be added
            res = testMap.assignFileAddressDefault(f6);
            Assert.AreEqual(res, true);
            res = testMap.insert(f6);
            Assert.AreEqual(res, true);
            f7 = new Apx.File("test7.out", 100);
            res = testMap.assignFileAddressDefault(f7);
            res = testMap.insert(f7);
            Assert.AreEqual(res, true);
            f8 = new Apx.File("test8.out", 1234);
            res = testMap.assignFileAddressDefault(f8);
            Assert.AreEqual(res, true);
            res = testMap.insert(f8);
            Assert.AreEqual(res, true);

            List<uint> addresses = testMap.getAddressList();
            List<string> names = testMap.getNameList();

            Assert.AreEqual(testMap.fileList.Count, 6);
            Assert.AreEqual(testMap.fileList[0], f1);
            Assert.AreEqual(testMap.fileList[1], f2);
            Assert.AreEqual(testMap.fileList[2], f3);
            Assert.AreEqual(testMap.fileList[3], f7);
            Assert.AreEqual(testMap.fileList[4], f8);
            Assert.AreEqual(testMap.fileList[5], f6);
        }

    }
}
