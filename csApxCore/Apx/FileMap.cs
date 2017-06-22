using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteFile;


namespace Apx
{
    public class Constants
    {
        public const uint PORT_DATA_START = 0x0;
        public const uint PORT_DATA_BOUNDARY = 0x400; //1KB, this must be a power of 2
        public const uint DEFINITION_START = 0x4000000; //64MB, this must be a power of 2
        public const uint DEFINITION_BOUNDARY = 0x100000; //1MB, this must be a power of 2
        public const uint USER_DATA_START = 0x20000000; //512MB, this must be a power of 2
        public const uint USER_DATA_END = 0x3FFFFC00; //Start of remote file cmd message area
        public const uint USER_DATA_BOUNDARY = 0x100000; //1MB, this must be a power of 2
        //public const string definition = "APX/1.2\nN\"dummyNode\"\nP\"dummyProvidePort\"C\nR\"WheelBasedVehicleSpeed\"S:=65535\n\n";
        //public const string definition = "APX/1.2\nN\"csApxClient\"\nP\"dummyProvidePort\"C\nR\"WheelBasedVehicleSpeed\"S:=65535\nR\"VehicleMode\"C:=255\n\n";
        public const string defaultDefinitionPath = "C:\\1_Provning\\APX\\cs-apx\\cs-apx\\bin\\Debug\\ApxDefinition.txt";
        
    }

    public class FileMap : RemoteFile.FileMap
    {
        public List<File> fileList = new List<File>();
        protected File lastMatch;
        // _keys is not used in the c# version of Apx

        public bool insert(RemoteFile.File file)
        {
            bool res = false;
            // Is this even needed anymore?
            if (file.address == uint.MaxValue)
            {
                res = assignFileAddressDefault(file);
                if (res)
                { sortedAddFileToList((Apx.File)file); }
                // else return false;
            }
            else
            { 
                sortedAddFileToList((Apx.File)file);
                res = true;
            }

            return res;
        }
        
        public bool remove(RemoteFile.File file)
        {
            // Is this even needed anymore?
            // Typecast RemoteFile.File to Apx.File
            Apx.File apxFile = (Apx.File)file;
            throw new System.NotImplementedException("remove not implemented");
        }
        
        public Apx.File findByAddress(uint address)
        {
            if ((lastMatch != null) && (address >= lastMatch.address) && (address < lastMatch.address + lastMatch.length))
            {
                return lastMatch;
            }
            foreach (File file in fileList)
            {
                if ((address >= file.address) && (address < file.address + file.length))
                {
                    lastMatch = file;
                    return file;
                }
            }
            return null;
        }

        public bool assignFileAddressDefault(RemoteFile.File file)
        {
            bool res = false;
            // Typecast RemoteFile.File to Apx.File
            Apx.File apxFile = (Apx.File)file;

            if (apxFile.name.EndsWith(".in") || apxFile.name.EndsWith(".out"))
            {
                res = assignFileAddress(apxFile, Constants.PORT_DATA_START, Constants.DEFINITION_START, Constants.PORT_DATA_BOUNDARY);
            }
            else if (apxFile.name.EndsWith(".apx"))
            {
                res = assignFileAddress(apxFile, Constants.DEFINITION_START, Constants.USER_DATA_START, Constants.DEFINITION_BOUNDARY);
            }
            else
            {
                res = assignFileAddress(apxFile, Constants.USER_DATA_START, Constants.USER_DATA_END, Constants.USER_DATA_BOUNDARY);
            }
            return res;
        }

        public bool assignFileAddress(File file, uint startAddress, uint endAddress, uint addressBoundary)
        {
            // startAddress must be a power of 2
            if ((startAddress != 0) && ((startAddress & (startAddress -1)) != 0) )
            { throw new System.ArgumentException("startAddress must be a power of 2"); }
            if (endAddress < startAddress)
            { throw new System.ArgumentException("endSection must be larger than startSection"); }
            if ((addressBoundary & (addressBoundary - 1)) != 0)
            { throw new System.ArgumentException("addressBoundary must be a power of 2"); }

            uint inFileTotLength = (uint)file.length + addressBoundary;
            uint possiblePlacement = startAddress;
            uint tempFileAddress;
            uint tempFileLength;

            // Tick once if there are no items in list
            for (int i = 0; i <= fileList.Count; i++)
            {
                if (fileList.Count == 0) // No files added yet
                {
                    tempFileAddress = endAddress;
                    tempFileLength = 0; 
                }
                else if ((i == 0) && (fileList[i].address >= endAddress))
                {
                    // For the first step the tempFile might be in a segment with higher address
                    // than the one we are interested in
                    tempFileAddress = endAddress;
                    tempFileLength = 0;
                }
                else if (i < fileList.Count)
                {
                    tempFileAddress = fileList[i].address;
                    tempFileLength = fileList[i].length;
                }
                else    // i = fileList.Count, last tick
                {
                    tempFileAddress = endAddress;
                    tempFileLength = 0;
                }
                
                // if possiblePlacement has a real value there is a slot after the previous file which can fit the file. 
                // if there is no file within file size + boundary the file fits here, otherwise continue searching.
                // This check is before assigning a possiblePlacement value to check that it doesn't write into the next filearea

                // tempFile is in the correct block
                if ((tempFileAddress >= startAddress) && (tempFileAddress < endAddress))  
                {
                    if (possiblePlacement + inFileTotLength <= tempFileAddress)
                    { break; }  // acceptable position found
                    else    
                    {
                        // Try with the next possible slot
                        if (tempFileAddress + (uint)tempFileLength < endAddress)
                        {
                            possiblePlacement = getNextArea(tempFileAddress, tempFileLength, addressBoundary);
                        }
                        else
                        { break; }
                    }
                }
            }
            if ((possiblePlacement >= startAddress) && (possiblePlacement + inFileTotLength < endAddress))
            { 
                file.address = possiblePlacement;
                return true; 
            }
            else
            { return false; }
        }

        public void sortedAddFileToList(File file)
        {
            fileList.Add(file);
            fileList.Sort((a, b) => a.address.CompareTo(b.address));
        }

        public List<string> getNameList()
        {
            List<string> temp = new List<string>();
            for (int i = 0; i < fileList.Count; i++)
            {
                temp.Add(fileList[i].name);
            }
            return temp;
        }

        public List<uint> getAddressList()
        {
            List<uint> temp = new List<uint>();
            for (int i = 0; i < fileList.Count; i++)
            {
                temp.Add(fileList[i].address);
            }
            return temp;
        }

        public uint getNextArea(uint fileAddress, uint fileLength, uint boundary)
        {
            uint tempAddress = fileAddress;
            if (boundary == 0 || boundary == uint.MaxValue)
            {
                throw new System.ArgumentException("addressBoundary must be a power of 2");
            }
            while (tempAddress < (fileAddress + (uint)fileLength))
            {
                tempAddress += boundary;
            }
            return tempAddress;
        }

    }
}
