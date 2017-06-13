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
        public void testFileMapOrder1()
        {
            /*
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
            **/
            throw new NotImplementedException("ToDo");

        }

        [TestMethod]
        public void testFileMapOrder2()
        {
            /*
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

            List<uint> addresses = testMap.getAddressList();
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
            */
            throw new NotImplementedException("ToDo");
        }

        [TestMethod]
        public void testFileMapAreaFull()
        {
            /*
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

            List<uint> addresses = testMap.getAddressList();
            List<string> names = testMap.getNameList();

            Assert.AreEqual(testMap._items.Count, 6);
            Assert.AreEqual(testMap._items[0], f1);
            Assert.AreEqual(testMap._items[1], f2);
            Assert.AreEqual(testMap._items[2], f3);
            Assert.AreEqual(testMap._items[3], f7);
            Assert.AreEqual(testMap._items[4], f8);
            Assert.AreEqual(testMap._items[5], f6);
             */
            throw new NotImplementedException("ToDo");
        }

    }
}
