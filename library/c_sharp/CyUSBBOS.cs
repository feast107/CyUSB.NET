/*
 ## Cypress CyUSB C# library source file (CyUSBBOS.cs)
 ## =======================================================
 ##
 ##  Copyright Cypress Semiconductor Corporation, 2009-2012,
 ##  All Rights Reserved
 ##  UNPUBLISHED, LICENSED SOFTWARE.
 ##
 ##  CONFIDENTIAL AND PROPRIETARY INFORMATION
 ##  WHICH IS THE PROPERTY OF CYPRESS.
 ##
 ##  Use of this file is governed
 ##  by the license agreement included in the file
 ##
 ##  <install>/license/license.rtf
 ##
 ##  where <install> is the Cypress software
 ##  install root directory path.
 ##
 ## =======================================================
*/
using System;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;

namespace CyUSB
{
    public class CyUSBBOS
    {
        public CyBOS_USB20_DEVICE_EXT USB20_DeviceExt;
        public CyBOS_SS_DEVICE_CAPABILITY SS_DeviceCap;
        public CyBOS_CONTAINER_ID Container_ID;
        private InvalidDeviceCapability InvalidDevCap;

        protected byte _bLength;/* Descriptor length*/
        public    byte Lenght => _bLength;

        protected byte _bDescriptorType;/* Descriptor Type */
        public    byte DescriptorType => _bDescriptorType;

        protected ushort _wToatalLength;/* Total length of descriptor ( icluding device capability*/
        public    ushort ToatalLength => _wToatalLength;

        protected byte _bNumDeviceCaps;/* Number of device capability descriptors in BOS  */
        public    byte NumDeviceCaps => _bNumDeviceCaps;

        internal unsafe CyUSBBOS(IntPtr handle, byte[] BosDescrData)
        {
            // initialize to null
            USB20_DeviceExt = null;
            SS_DeviceCap = null;
            Container_ID = null;


            // parse the Bos Descriptor data
            fixed (byte* buf = BosDescrData)
            {
                var BosDesc = (USB_BOS_DESCRIPTOR*)buf;
                _bLength = BosDesc->bLength;
                _bDescriptorType = BosDesc->bDescriptorType;
                _bNumDeviceCaps = BosDesc->bNumDeviceCaps;
                _wToatalLength = BosDesc->wToatalLength;

                int totallen = _wToatalLength;
                totallen -= BosDesc->bLength;

                if (totallen < 0)
                    return;

                var DevCap = (byte*)(buf + BosDesc->bLength); // get nex descriptor

                for (var i = 0; i < _bNumDeviceCaps; i++)
                {
                    //check capability type
                    switch (DevCap[2])
                    {
                        case CyConst.USB_BOS_CAPABILITY_TYPE_USB20_EXT:
                            {
                                var USB20_ext = (USB_BOS_USB20_DEVICE_EXTENSION*)DevCap;
                                totallen -= USB20_ext->bLength;
                                DevCap = (byte*)DevCap + USB20_ext->bLength;
                                USB20_DeviceExt = new CyBOS_USB20_DEVICE_EXT(handle, USB20_ext);
                                break;
                            }
                        case CyConst.USB_BOS_CAPABILITY_TYPE_SUPERSPEED_USB:
                            {
                                var SS_Capability = (USB_BOS_SS_DEVICE_CAPABILITY*)DevCap;
                                totallen -= SS_Capability->bLength;
                                DevCap = (byte*)DevCap + SS_Capability->bLength;
                                SS_DeviceCap = new CyBOS_SS_DEVICE_CAPABILITY(handle, SS_Capability);
                                break;
                            }
                        case CyConst.USB_BOS_CAPABILITY_TYPE_CONTAINER_ID:
                            {
                                var USB_ContainerID = (USB_BOS_CONTAINER_ID*)DevCap;
                                totallen -= USB_ContainerID->bLength;
                                DevCap = (byte*)DevCap + USB_ContainerID->bLength;
                                Container_ID = new CyBOS_CONTAINER_ID(handle, USB_ContainerID);
                                break;
                            }
                        default:
                            {
                                InvalidDevCap = new InvalidDeviceCapability();
                                break;
                            }
                    }
                    if (totallen < 0)
                        break;
                }

            }
        }

        public TreeNode Tree
        {
            get
            {
                var tmp = "BOS";

                var iTree = new TreeNode[NumDeviceCaps];

                for (var i = 0; i < NumDeviceCaps; i++)
                {
                    if (USB20_DeviceExt != null && i == 0)
                        iTree[i] = USB20_DeviceExt.Tree;
                    else if (SS_DeviceCap != null && i == 1)
                        iTree[i] = SS_DeviceCap.Tree;
                    else if (Container_ID != null && i == 2)
                        iTree[i] = Container_ID.Tree;
                    else
                    {
                        iTree[i] = InvalidDevCap.Tree;
                    }
                }

                var t = new TreeNode(tmp, iTree)
                {
                    Tag = this
                };

                return t;
            }

        }

        public override string ToString()
        {
            var s = new StringBuilder("\t<BOS>\r\n");

            s.Append($"\t\tNumberOfDeviceCapability=\"{_bNumDeviceCaps:X2}h\"\r\n");
            s.Append($"\t\tDescriptorType=\"{_bDescriptorType}\"\r\n");
            s.Append($"\t\tDescriptorLength=\"{_bLength}\"\r\n");
            s.Append($"\t\tTotalLength=\"{_wToatalLength}\"\r\n");
            for (var i = 0; i < NumDeviceCaps; i++)
            {
                if (USB20_DeviceExt != null && i == 0)
                    s.Append(USB20_DeviceExt.ToString());
                else if (SS_DeviceCap != null && i == 1)
                    s.Append(SS_DeviceCap.ToString());
                else if (Container_ID != null && i == 2)
                    s.Append(Container_ID.ToString());
                else
                    s.Append(InvalidDevCap.ToString());

            }
            s.Append("\t</BOS>\r\n");
            return s.ToString();
        }
    }

    // This class defined to handle invalid BOS descriptor table configuration
    public class InvalidDeviceCapability
    {
        public TreeNode Tree
        {
            get
            {
                var tmp = "Invalid Device Capability";
                var t   = new TreeNode(tmp)
                {
                    Tag = this
                };
                return t;
            }
        }
        public override string ToString()
        {
            var s = new StringBuilder("\t\t<Please correct your BOS descriptor table in firmware>\r\n");
            return s.ToString();
        }
    }

    public class CyBOS_USB20_DEVICE_EXT
    {

        protected byte _bLength;/* Descriptor length*/
        public    byte Lenght => _bLength;

        protected byte _bDescriptorType;/* Descriptor Type */
        public    byte DescriptorType => _bDescriptorType;

        protected byte _bDevCapabilityType;/* Device capability type*/
        public    byte DevCapabilityType => _bDevCapabilityType;

        protected uint _bmAttribute;// Bitmap encoding for supprted feature and  Link power managment supprted if set
        public    uint bmAttribute => _bmAttribute;

        internal unsafe CyBOS_USB20_DEVICE_EXT(IntPtr handle, USB_BOS_USB20_DEVICE_EXTENSION* USB20_DeviceExt)
        {
            _bLength = USB20_DeviceExt->bLength;
            _bDescriptorType = USB20_DeviceExt->bDescriptorType;
            _bDevCapabilityType = USB20_DeviceExt->bDevCapabilityType;
            _bmAttribute = USB20_DeviceExt->bmAttribute;
        }

        public TreeNode Tree
        {
            get
            {
                var tmp = "USB20 Device Extension";
                var t   = new TreeNode(tmp)
                {
                    Tag = this
                };
                return t;
            }

        }
        public override string ToString()
        {
            var s = new StringBuilder("\t\t<USB20 Device Extension>\r\n");

            s.Append($"\t\t\tDescriptorLength=\"{_bLength}\"\r\n");
            s.Append($"\t\t\tDescriptorType=\"{_bDescriptorType}\"\r\n");
            s.Append($"\t\t\tDeviceCapabilityType=\"{_bDevCapabilityType}\"\r\n");
            s.Append($"\t\t\tbmAttribute=\"{_bmAttribute:X2}h\"\r\n");
            s.Append("\t\t</USB20 Device Extension>\r\n");
            return s.ToString();
        }
    }

    public class CyBOS_SS_DEVICE_CAPABILITY
    {
        protected byte _bLength;/* Descriptor length*/
        public    byte Lenght => _bLength;

        protected byte _bDescriptorType;/* Descriptor Type */
        public    byte DescriptorType => _bDescriptorType;

        protected byte _bDevCapabilityType;/* Device capability type*/
        public    byte DevCapabilityType => _bDevCapabilityType;

        protected byte _bmAttribute;// Bitmap encoding for supprted feature and  Link power managment supprted if set
        public    byte bmAttribute => _bmAttribute;

        protected ushort _wSpeedsSuported;//low speed supported if set,full speed supported if set,high speed supported if set,super speed supported if set,15:4 nt used
        public    ushort SpeedsSuported => _wSpeedsSuported;

        protected byte _bFunctionalitySupporte;
        public    byte FunctionalitySupporte => _bFunctionalitySupporte;

        protected byte _bU1DevExitLat;//U1 device exit latency
        public    byte U1DevExitLat => _bU1DevExitLat;

        protected ushort _bU2DevExitLat;//U2 device exit latency
        public    ushort U2DevExitLat => _bU2DevExitLat;

        internal unsafe CyBOS_SS_DEVICE_CAPABILITY(IntPtr handle, USB_BOS_SS_DEVICE_CAPABILITY* USB_SuperSpeedUsb)
        {
            _bLength = USB_SuperSpeedUsb->bLength;
            _bDescriptorType = USB_SuperSpeedUsb->bDescriptorType;
            _bDevCapabilityType = USB_SuperSpeedUsb->bDevCapabilityType;
            _bFunctionalitySupporte = USB_SuperSpeedUsb->bFunctionalitySupporte;
            _bmAttribute = USB_SuperSpeedUsb->bmAttribute;
            _bU1DevExitLat = USB_SuperSpeedUsb->bU1DevExitLat;
            _bU2DevExitLat = USB_SuperSpeedUsb->bU2DevExitLat;
        }
        public TreeNode Tree
        {
            get
            {
                var tmp = "SuperSpeed Device capability";
                var t   = new TreeNode(tmp)
                {
                    Tag = this
                };
                return t;
            }

        }
        public override string ToString()
        {
            var s = new StringBuilder("\t\t<SUPERSPEED USB>\r\n");

            s.Append($"\t\t\tDescriptorLength=\"{_bLength}\"\r\n");
            s.Append($"\t\t\tDescriptorType=\"{_bDescriptorType}\"\r\n");
            s.Append($"\t\t\tDeviceCapabilityType=\"{_bDevCapabilityType}\"\r\n");
            s.Append($"\t\t\tFunctionalitySupporte=\"{_bFunctionalitySupporte}\"\r\n");
            s.Append($"\t\t\tbmAttribute=\"{_bmAttribute:X2}h\"\r\n");
            s.Append($"\t\t\tU1Device Exit Latency=\"{_bU1DevExitLat}\"\r\n");
            s.Append($"\t\t\tU2Device Exit Latency=\"{_bU2DevExitLat:X2}h\"\r\n");
            s.Append("\t\t</SUPERSPEED USB>\r\n");
            return s.ToString();
        }
    }

    public class CyBOS_CONTAINER_ID
    {
        protected byte _bLength;/* Descriptor length*/
        public    byte Lenght => _bLength;

        protected byte _bDescriptorType;/* Descriptor Type */
        public    byte DescriptorType => _bDescriptorType;

        protected byte _bDevCapabilityType;/* Device capability type*/
        public    byte DevCapabilityType => _bDevCapabilityType;

        protected byte _bResrved; // no use
        public    byte Reserved => _bResrved;

        protected byte[] _ContainerID;/* UUID */
        public    byte[] ContainerID => _ContainerID;

        internal unsafe CyBOS_CONTAINER_ID(IntPtr handle, USB_BOS_CONTAINER_ID* USB_ContainerID)
        {
            _bLength = USB_ContainerID->bLength;
            _bDescriptorType = USB_ContainerID->bDescriptorType;
            _bDevCapabilityType = USB_ContainerID->bDevCapabilityType;
            _ContainerID = new byte[CyConst.USB_BOS_CAPABILITY_TYPE_CONTAINER_ID_SIZE];
            for (var i = 0; i < CyConst.USB_BOS_CAPABILITY_TYPE_CONTAINER_ID_SIZE; i++)
                _ContainerID[i] = USB_ContainerID->ContainerID[i];

        }
        public TreeNode Tree
        {
            get
            {
                var tmp = "Container ID";
                var t   = new TreeNode(tmp)
                {
                    Tag = this
                };
                return t;
            }

        }
        public override string ToString()
        {
            var s = new StringBuilder("\t\t<CONTAINER ID>\r\n");

            s.Append($"\t\t\tDescriptorLength=\"{_bLength}\"\r\n");
            s.Append($"\t\t\tDescriptorType=\"{_bDescriptorType}\"\r\n");
            s.Append($"\t\t\tDeviceCapabilityType=\"{_bDevCapabilityType}\"\r\n");
            //s.Append(string.Format("\t\tbmAttribute=\"{0:X2}h\"\r\n", _ContainerID.));            
            s.Append("\t\t</CONTAINER ID>\r\n");
            return s.ToString();
        }
    }
}
