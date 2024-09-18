/*
 ## Cypress CyUSB C# library source file (CyUSBInterface.cs)
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
using System.Windows.Forms;
using System.Text;

namespace CyUSB
{
    public class CyUSBInterfaceContainer
    {
        public CyUSBInterface[] Interfaces;

        byte        _bInterfaceNumber;
        public byte bInterfaceNumber => _bInterfaceNumber;

        byte        _AltInterfacesCount;
        public byte AltInterfacesCount => _AltInterfacesCount;

        public CyUSBInterfaceContainer(byte intfcNum, byte altIntfcCount)
        {
            _bInterfaceNumber = intfcNum;
            _AltInterfacesCount = altIntfcCount;
            Interfaces = new CyUSBInterface[altIntfcCount];
        }

        public TreeNode Tree
        {
            get
            {
                var itmp = "Interface " + bInterfaceNumber.ToString();
                var altTree = new TreeNode[AltInterfacesCount];
                for (var i = 0; i < AltInterfacesCount; i++)
                {
                    altTree[i] = Interfaces[i].Tree;
                }
                var iNode = new TreeNode(itmp, altTree)
                {
                    Tag = this
                };

                return iNode;
            }
        }

        public override string ToString()
        {
            var s = new StringBuilder("\t<INTERFACE " + bInterfaceNumber.ToString() + ">\r\n");

            for (var i = 0; i < AltInterfacesCount; i++)
                s.Append(Interfaces[i].ToString());

            s.Append("\t<INTERFACE " + bInterfaceNumber.ToString() + ">\r\n");
            return s.ToString();
        }

    }

    /// <summary>
    /// The CyUSBInterface Class
    /// </summary>
    public class CyUSBInterface
    {
        public CyUSBEndPoint[] EndPoints;  // Holds pointers to all the interface's endpoints, plus a pointer to the Control endpoint zero

        byte        _bLength;
        public byte bLength => _bLength;

        byte        _bDescriptorType;
        public byte bDescriptorType => _bDescriptorType;

        byte        _bInterfaceNumber;
        public byte bInterfaceNumber => _bInterfaceNumber;

        byte        _bAlternateSetting;
        public byte bAlternateSetting => _bAlternateSetting;

        byte        _bNumEndpoints;           // Not counting the control endpoint
        public byte bNumEndpoints => _bNumEndpoints;

        byte        _bInterfaceClass;
        public byte bInterfaceClass => _bInterfaceClass;

        byte        _bInterfaceSubClass;
        public byte bInterfaceSubClass => _bInterfaceSubClass;

        byte        _bInterfaceProtocol;
        public byte bInterfaceProtocol => _bInterfaceProtocol;

        byte        _iInterface;
        public byte iInterface => _iInterface;

        internal byte _bAltSettings;
        public   byte bAltSettings => _bAltSettings;

        ushort        _wTotalLength;          // Needed in case Intfc has additional (non-endpt) descriptors
        public ushort wTotalLength => _wTotalLength;

        internal unsafe CyUSBInterface(IntPtr handle, byte* DescrData, CyControlEndPoint ctlEndPt)
        {

            var pIntfcDescriptor = (USB_INTERFACE_DESCRIPTOR*)DescrData;


            _bLength = pIntfcDescriptor->bLength;
            _bDescriptorType = pIntfcDescriptor->bDescriptorType;
            _bInterfaceNumber = pIntfcDescriptor->bInterfaceNumber;
            _bAlternateSetting = pIntfcDescriptor->bAlternateSetting;
            _bNumEndpoints = pIntfcDescriptor->bNumEndpoints;
            _bInterfaceClass = pIntfcDescriptor->bInterfaceClass;
            _bInterfaceSubClass = pIntfcDescriptor->bInterfaceSubClass;
            _bInterfaceProtocol = pIntfcDescriptor->bInterfaceProtocol;
            _iInterface = pIntfcDescriptor->iInterface;

            _bAltSettings = 0;
            _wTotalLength = bLength;

            var desc = (byte*)(DescrData + pIntfcDescriptor->bLength);

            int i;
            var unexpected = 0;

            EndPoints = new CyUSBEndPoint[bNumEndpoints + 1];
            EndPoints[0] = ctlEndPt;

            for (i = 1; i <= bNumEndpoints; i++)
            {

                var endPtDesc = (USB_ENDPOINT_DESCRIPTOR*)desc;
                _wTotalLength += endPtDesc->bLength;


                if (endPtDesc->bDescriptorType == CyConst.USB_ENDPOINT_DESCRIPTOR_TYPE)
                {
                    switch (endPtDesc->bmAttributes)
                    {
                        case 0:
                            EndPoints[i] = ctlEndPt;
                            break;
                        case 1:
                            EndPoints[i] = new CyIsocEndPoint(handle, endPtDesc);
                            break;
                        case 2:
                            EndPoints[i] = new CyBulkEndPoint(handle, endPtDesc);
                            break;
                        case 3:
                            EndPoints[i] = new CyInterruptEndPoint(handle, endPtDesc);
                            break;
                    }

                    desc += endPtDesc->bLength;
                }
                else
                {
                    unexpected++;
                    if (unexpected < 12)
                    {  // Sanity check - prevent infinite loop

                        // This may have been a class-specific descriptor (like HID).  Skip it.
                        desc += endPtDesc->bLength;

                        // Stay in the loop, grabbing the next descriptor
                        i--;
                    }

                }

            }
        }
        internal unsafe CyUSBInterface(IntPtr handle, byte* DescrData, CyControlEndPoint ctlEndPt, byte usb30dummy)
        {

            var pIntfcDescriptor = (USB_INTERFACE_DESCRIPTOR*)DescrData;


            _bLength = pIntfcDescriptor->bLength;
            _bDescriptorType = pIntfcDescriptor->bDescriptorType;
            _bInterfaceNumber = pIntfcDescriptor->bInterfaceNumber;
            _bAlternateSetting = pIntfcDescriptor->bAlternateSetting;
            _bNumEndpoints = pIntfcDescriptor->bNumEndpoints;
            _bInterfaceClass = pIntfcDescriptor->bInterfaceClass;
            _bInterfaceSubClass = pIntfcDescriptor->bInterfaceSubClass;
            _bInterfaceProtocol = pIntfcDescriptor->bInterfaceProtocol;
            _iInterface = pIntfcDescriptor->iInterface;

            _bAltSettings = 0;
            _wTotalLength = bLength;

            var desc = (byte*)(DescrData + pIntfcDescriptor->bLength);

            int i;
            var unexpected = 0;

            EndPoints = new CyUSBEndPoint[bNumEndpoints + 1];
            EndPoints[0] = ctlEndPt;

            for (i = 1; i <= bNumEndpoints; i++)
            {

                var bSSDec = false;
                var endPtDesc = (USB_ENDPOINT_DESCRIPTOR*)desc;
                desc += endPtDesc->bLength;
                var ssendPtDesc = (USB_SUPERSPEED_ENDPOINT_COMPANION_DESCRIPTOR*)desc;
                _wTotalLength += endPtDesc->bLength;

                if (ssendPtDesc != null)
                    bSSDec = ssendPtDesc->bDescriptorType == CyConst.USB_SUPERSPEED_ENDPOINT_COMPANION;


                if (endPtDesc->bDescriptorType == CyConst.USB_ENDPOINT_DESCRIPTOR_TYPE && bSSDec)
                {
                    switch (endPtDesc->bmAttributes)
                    {
                        case 0:
                            EndPoints[i] = ctlEndPt;
                            break;
                        case 1:
                            EndPoints[i] = new CyIsocEndPoint(handle, endPtDesc, ssendPtDesc);
                            break;
                        case 2:
                            EndPoints[i] = new CyBulkEndPoint(handle, endPtDesc, ssendPtDesc);
                            break;
                        case 3:
                            EndPoints[i] = new CyInterruptEndPoint(handle, endPtDesc, ssendPtDesc);
                            break;
                    }
                    _wTotalLength += ssendPtDesc->bLength;
                    desc += ssendPtDesc->bLength;
                }
                else if (endPtDesc->bDescriptorType == CyConst.USB_ENDPOINT_DESCRIPTOR_TYPE)
                {
                    switch (endPtDesc->bmAttributes)
                    {
                        case 0:
                            EndPoints[i] = ctlEndPt;
                            break;
                        case 1:
                            EndPoints[i] = new CyIsocEndPoint(handle, endPtDesc);
                            break;
                        case 2:
                            EndPoints[i] = new CyBulkEndPoint(handle, endPtDesc);
                            break;
                        case 3:
                            EndPoints[i] = new CyInterruptEndPoint(handle, endPtDesc);
                            break;
                    }
                }
                else
                {
                    unexpected++;
                    if (unexpected < 12)
                    {  // Sanity check - prevent infinite loop

                        // This may have been a class-specific descriptor (like HID).  Skip it.
                        desc += endPtDesc->bLength;

                        // Stay in the loop, grabbing the next descriptor
                        i--;
                    }

                }

            }
        }

        public TreeNode Tree
        {
            get
            {
                var tmp = "Alternate Setting " + bAlternateSetting.ToString();

                //string tmp = "Interface " + bInterfaceNumber.ToString();

                var eTree = new TreeNode[_bNumEndpoints];
                for (var i = 0; i < _bNumEndpoints; i++)
                    eTree[i] = EndPoints[i + 1].Tree;

                var t = new TreeNode(tmp, eTree)
                {
                    Tag = this
                };

                return t;
            }

        }

        public override string ToString()
        {
            var s = new StringBuilder("\t\t<INTERFACE>\r\n");

            s.Append($"\t\t\tInterface=\"{_iInterface}\"\r\n");
            s.Append($"\t\t\tInterfaceNumber=\"{_bInterfaceNumber}\"\r\n");
            s.Append($"\t\t\tAltSetting=\"{_bAlternateSetting}\"\r\n");
            s.Append($"\t\t\tClass=\"{_bInterfaceClass:X2}h\"\r\n");
            s.Append($"\t\t\tSubclass=\"{_bInterfaceSubClass:X2}h\"\r\n");
            s.Append($"\t\t\tProtocol=\"{_bInterfaceProtocol}\"\r\n");
            s.Append($"\t\t\tEndpoints=\"{_bNumEndpoints}\"\r\n");
            s.Append($"\t\t\tDescriptorType=\"{_bDescriptorType}\"\r\n");
            s.Append($"\t\t\tDescriptorLength=\"{_bLength}\"\r\n");

            for (var i = 0; i < _bNumEndpoints; i++)
                s.Append(EndPoints[i + 1].ToString());

            s.Append("\t\t</INTERFACE>\r\n");
            return s.ToString();
        }


    }
}
