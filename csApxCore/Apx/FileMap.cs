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
        public const ulong PORT_DATA_START     = 0x0;
        public const ulong PORT_DATA_BOUNDARY  = 0x400; //1KB, this must be a power of 2
        public const ulong DEFINITION_START    = 0x4000000; //64MB, this must be a power of 2
        public const ulong DEFINITION_BOUNDARY = 0x100000; //1MB, this must be a power of 2
        public const ulong USER_DATA_START     = 0x20000000; //512MB, this must be a power of 2
        public const ulong USER_DATA_END       = 0x3FFFFC00; //Start of remote file cmd message area
        public const ulong USER_DATA_BOUNDARY  = 0x100000; //1MB, this must be a power of 2
    }

    public class File : RemoteFile.File
    {

    }

    public class FileMap : RemoteFile.FileMap
    {
        public List<File> _items = new List<File>();
        //public List<ulong> _keys = new List<ulong>();

        // define index

        public bool insert(RemoteFile.File file)
        {
            return assignFileAddressDefault(file);
        }
        
        public bool remove(RemoteFile.File file)
        {
            // Typecast RemoteFile.File to Apx.File
            File apxFile = (File)file;
            throw new System.NotImplementedException("remove not implemented");
        }

        public bool assignFileAddressDefault(RemoteFile.File file)
        {
            bool res = false;
            // Typecast RemoteFile.File to Apx.File
            File apxFile = (File)file;

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

        public bool assignFileAddress(File file, ulong startAddress, ulong endAddress, ulong addressBoundary)
        {
            // startAddress must be a power of 2
            if ((startAddress != 0) && ((startAddress & (startAddress -1)) != 0) )
            { throw new System.ArgumentException("startAddress must be a power of 2"); }
            if (endAddress < startAddress)
            { throw new System.ArgumentException("endSection must be larger than startSection"); }
            if ((addressBoundary & (addressBoundary - 1)) != 0)
            { throw new System.ArgumentException("addressBoundary must be a power of 2"); }

            //List<ulong> checkOrderList = new List<ulong>();
            ulong inFileTotLength = (ulong)file.length + addressBoundary;
            ulong possiblePlacement = startAddress;
            ulong tempFileAddress;
            int tempFileLength;

            // Tick once if there are no items in list
            for (int i = 0; i <= _items.Count; i++)
            {
                if (_items.Count == 0) // No files added yet
                {
                    tempFileAddress = endAddress;
                    tempFileLength = 0; 
                }
                else if ((i == 0) && (_items[i].address >= endAddress))
                {
                    // For the first step the tempFile might be in a segment with higher address
                    // than the one we are interested in
                    tempFileAddress = endAddress;
                    tempFileLength = 0;
                }
                else if (i < _items.Count)
                {
                    tempFileAddress = _items[i].address;
                    tempFileLength = _items[i].length;
                }
                else    // i = _items.Count, last tick
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
                        if (tempFileAddress + (ulong)tempFileLength < endAddress)
                        {
                            // possiblePlacement = tempFileAddress + (ulong)tempFileLength;
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
                sortedAddFileToList(file);
                return true; 
            }
            else
            { return false; }
        }

        public void sortedAddFileToList(File file)
        {
            _items.Add(file);
            _items.Sort((a, b) => a.address.CompareTo(b.address));
        }

        public List<string> getNameList()
        {
            List<string> temp = new List<string>();
            for (int i = 0; i < _items.Count; i++)
            {
                temp.Add(_items[i].name);
            }
            return temp;
        }

        public List<ulong> getAddressList()
        {
            List<ulong> temp = new List<ulong>();
            for (int i = 0; i < _items.Count; i++)
            {
                temp.Add(_items[i].address);
            }
            return temp;
        }

        public ulong getNextArea(ulong fileAddress, int fileLength, ulong boundary)
        {
            ulong tempAddress = fileAddress;
            if (boundary == 0 || boundary == ulong.MaxValue)
            {
                throw new System.ArgumentException("addressBoundary must be a power of 2");
            }
            while (tempAddress < (fileAddress + (ulong)fileLength))
            {
                tempAddress += boundary;
            }
            return tempAddress;
        }

    }
}
