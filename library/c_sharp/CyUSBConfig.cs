/*
 ## Cypress CyUSB C# library source file (CyUSBConfig.cs)
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
    /// <summary>
    /// The CyUSBConfig Class
    /// </summary>
    public class CyUSBConfig
    {
        public CyUSBInterface[] Interfaces;

        CyUSBInterfaceContainer[] IntfcContainer;

        byte        _bLength;
        public byte bLength => _bLength;

        byte        _bDescriptorType;
        public byte bDescriptorType => _bDescriptorType;

        ushort        _wTotalLength;
        public ushort wTotalLength => _wTotalLength;

        byte        _bNumInterfaces;
        public byte bNumInterfaces => _bNumInterfaces;

        byte        _bConfigurationValue;
        public byte bConfigurationValue => _bConfigurationValue;

        byte        _iConfiguration;
        public byte iConfiguration => _iConfiguration;

        byte        _bmAttributes;
        public byte bmAttributes => _bmAttributes;

        byte        _MaxPower;
        public byte MaxPower => _MaxPower;

        byte        _AltInterfaces;
        public byte AltInterfaces => _AltInterfaces;

        internal unsafe CyUSBConfig(IntPtr handle, byte[] DescrData, CyControlEndPoint ctlEndPt)
        {// This contructore is to initialize usb2.0 device
            fixed (byte* buf = DescrData)
            {
                var ConfigDescr = (USB_CONFIGURATION_DESCRIPTOR*)buf;

                _bLength = ConfigDescr->bLength;
                _bDescriptorType = ConfigDescr->bDescriptorType;
                _wTotalLength = ConfigDescr->wTotalLength;
                _bNumInterfaces = ConfigDescr->bNumInterfaces;
                _AltInterfaces = 0;
                _bConfigurationValue = ConfigDescr->bConfigurationValue;
                _iConfiguration = ConfigDescr->iConfiguration;
                _bmAttributes = ConfigDescr->bmAttributes;
                _MaxPower = ConfigDescr->MaxPower;

                int tLen = ConfigDescr->wTotalLength;

                var desc = (byte*)(buf + ConfigDescr->bLength);
                int bytesConsumed = ConfigDescr->bLength;

                Interfaces = new CyUSBInterface[CyConst.MAX_INTERFACES];

                var i = 0;
                do
                {
                    var interfaceDesc = (USB_INTERFACE_DESCRIPTOR*)desc;

                    if (interfaceDesc->bDescriptorType == CyConst.USB_INTERFACE_DESCRIPTOR_TYPE)
                    {
                        Interfaces[i] = new CyUSBInterface(handle, desc, ctlEndPt);
                        i++;
                        _AltInterfaces++;  // Actually the total number of interfaces for the config
                        bytesConsumed += Interfaces[i - 1].wTotalLength;
                    }
                    else
                    {
                        // Unexpected descriptor type
                        // Just skip it and go on  - could have thrown an exception instead
                        // since this indicates that the descriptor structure is invalid.
                        bytesConsumed += interfaceDesc->bLength;
                    }


                    desc = (byte*)(buf + bytesConsumed);

                } while (bytesConsumed < tLen && i < CyConst.MAX_INTERFACES);
                // Count the alt interfaces for each interface number
                for (i = 0; i < _AltInterfaces; i++)
                {
                    Interfaces[i]._bAltSettings = 0;

                    for (var j = 0; j < AltInterfaces; j++) // Walk the list looking for identical bInterfaceNumbers
                        if (Interfaces[i].bInterfaceNumber == Interfaces[j].bInterfaceNumber)
                            Interfaces[i]._bAltSettings++;

                }

                // Create the Interface Container (this is done only for Tree view purpose).
                IntfcContainer = new CyUSBInterfaceContainer[bNumInterfaces];

                var altDict = new Dictionary<int, bool>();
                var intfcCount = 0;

                for (i = 0; i < _AltInterfaces; i++)
                {
                    if (altDict.ContainsKey(Interfaces[i].bInterfaceNumber) == false)
                    {
                        var altIntfcCount = 0;
                        IntfcContainer[intfcCount] = new CyUSBInterfaceContainer(Interfaces[i].bInterfaceNumber, Interfaces[i].bAltSettings);

                        for (var j = i; j < AltInterfaces; j++)
                        {
                            if (Interfaces[i].bInterfaceNumber == Interfaces[j].bInterfaceNumber)
                            {
                                IntfcContainer[intfcCount].Interfaces[altIntfcCount] = Interfaces[j];
                                altIntfcCount++;
                            }
                        }
                        intfcCount++;
                        altDict.Add(Interfaces[i].bInterfaceNumber, true);
                    }

                }
            } /* end of fixed loop */

        }
        internal unsafe CyUSBConfig(IntPtr handle, byte[] DescrData, CyControlEndPoint ctlEndPt, byte usb30Dummy)
        {// This constructure will be called for USB3.0 device initialization
            fixed (byte* buf = DescrData)
            {
                var ConfigDescr = (USB_CONFIGURATION_DESCRIPTOR*)buf;

                _bLength = ConfigDescr->bLength;
                _bDescriptorType = ConfigDescr->bDescriptorType;
                _wTotalLength = ConfigDescr->wTotalLength;
                _bNumInterfaces = ConfigDescr->bNumInterfaces;
                _AltInterfaces = 0;
                _bConfigurationValue = ConfigDescr->bConfigurationValue;
                _iConfiguration = ConfigDescr->iConfiguration;
                _bmAttributes = ConfigDescr->bmAttributes;
                _MaxPower = ConfigDescr->MaxPower;

                int tLen = ConfigDescr->wTotalLength;

                var desc = (byte*)(buf + ConfigDescr->bLength);
                int bytesConsumed = ConfigDescr->bLength;

                Interfaces = new CyUSBInterface[CyConst.MAX_INTERFACES];

                var i = 0;
                do
                {
                    var interfaceDesc = (USB_INTERFACE_DESCRIPTOR*)desc;

                    if (interfaceDesc->bDescriptorType == CyConst.USB_INTERFACE_DESCRIPTOR_TYPE)
                    {
                        Interfaces[i] = new CyUSBInterface(handle, desc, ctlEndPt, usb30Dummy);
                        i++;
                        _AltInterfaces++;  // Actually the total number of interfaces for the config
                        bytesConsumed += Interfaces[i - 1].wTotalLength;
                    }
                    else
                    {
                        // Unexpected descriptor type
                        // Just skip it and go on  - could have thrown an exception instead
                        // since this indicates that the descriptor structure is invalid.
                        bytesConsumed += interfaceDesc->bLength;
                    }


                    desc = (byte*)(buf + bytesConsumed);

                } while (bytesConsumed < tLen && i < CyConst.MAX_INTERFACES);
                // Count the alt interfaces for each interface number
                for (i = 0; i < _AltInterfaces; i++)
                {
                    Interfaces[i]._bAltSettings = 0;

                    for (var j = 0; j < AltInterfaces; j++) // Walk the list looking for identical bInterfaceNumbers
                        if (Interfaces[i].bInterfaceNumber == Interfaces[j].bInterfaceNumber)
                            Interfaces[i]._bAltSettings++;

                }

                // Create the Interface Container (this is done only for Tree view purpose).
                IntfcContainer = new CyUSBInterfaceContainer[bNumInterfaces];

                var altDict = new Dictionary<int, bool>();
                var intfcCount = 0;

                for (i = 0; i < _AltInterfaces; i++)
                {
                    if (altDict.ContainsKey(Interfaces[i].bInterfaceNumber) == false)
                    {
                        var altIntfcCount = 0;
                        IntfcContainer[intfcCount] = new CyUSBInterfaceContainer(Interfaces[i].bInterfaceNumber, Interfaces[i].bAltSettings);

                        for (var j = i; j < AltInterfaces; j++)
                        {
                            if (Interfaces[i].bInterfaceNumber == Interfaces[j].bInterfaceNumber)
                            {
                                IntfcContainer[intfcCount].Interfaces[altIntfcCount] = Interfaces[j];
                                altIntfcCount++;
                            }
                        }
                        intfcCount++;
                        altDict.Add(Interfaces[i].bInterfaceNumber, true);
                    }

                }
            } /* end of fixed loop */

        }

        public TreeNode Tree
        {
            get
            {
                var tmp = "Configuration " + bConfigurationValue.ToString();
                //string tmp = "Primary Configuration";
                //if (iConfiguration == 1)
                //    tmp = "Secondary Configuration";

                //TreeNode[] iTree = new TreeNode[_AltInterfaces + 1];
                var iTree = new TreeNode[bNumInterfaces + 1];

                iTree[0]     = new TreeNode("Control endpoint (0x00)")
                {
                    Tag = Interfaces[0].EndPoints[0]
                };

                for (var i = 0; i < bNumInterfaces; i++)
                    iTree[i + 1] = IntfcContainer[i].Tree;

                //for (int i = 0; i < _AltInterfaces; i++)
                //    iTree[i + 1] = Interfaces[i].Tree;

                var t = new TreeNode(tmp, iTree)
                {
                    Tag = this
                };

                return t;
            }

        }

        public override string ToString()
        {
            var s = new StringBuilder("\t<CONFIGURATION>\r\n");

            s.Append($"\t\tConfiguration=\"{_iConfiguration}\"\r\n");
            s.Append($"\t\tConfigurationValue=\"{_bConfigurationValue}\"\r\n");
            s.Append($"\t\tAttributes=\"{_bmAttributes:X2}h\"\r\n");
            s.Append($"\t\tInterfaces=\"{_bNumInterfaces}\"\r\n");
            s.Append($"\t\tDescriptorType=\"{_bDescriptorType}\"\r\n");
            s.Append($"\t\tDescriptorLength=\"{_bLength}\"\r\n");
            s.Append($"\t\tTotalLength=\"{_wTotalLength}\"\r\n");
            s.Append($"\t\tMaxPower=\"{_MaxPower}\"\r\n");

            for (var i = 0; i < _AltInterfaces; i++)
                s.Append(Interfaces[i].ToString());

            s.Append("\t</CONFIGURATION>\r\n");
            return s.ToString();
        }


    }
}
