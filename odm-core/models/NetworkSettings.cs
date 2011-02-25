﻿#region License and Terms
//----------------------------------------------------------------------------------------------------------------
// Copyright (C) 2010 Synesis LLC and/or its subsidiaries. All rights reserved.
//
// Commercial Usage
// Licensees  holding  valid ONVIF  Device  Manager  Commercial  licenses may use this file in accordance with the
// ONVIF  Device  Manager Commercial License Agreement provided with the Software or, alternatively, in accordance
// with the terms contained in a written agreement between you and Synesis LLC.
//
// GNU General Public License Usage
// Alternatively, this file may be used under the terms of the GNU General Public License version 3.0 as published
// by  the Free Software Foundation and appearing in the file LICENSE.GPL included in the  packaging of this file.
// Please review the following information to ensure the GNU General Public License version 3.0 
// requirements will be met: http://www.gnu.org/copyleft/gpl.html.
// 
// If you have questions regarding the use of this file, please contact Synesis LLC at onvifdm@synesis.ru.
//----------------------------------------------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;

using odm.onvif;
using dev=onvif.services.device;


namespace odm.models {

	public class NetworkSettings{// : NotifyPropertyChangedBase<NetworkSettings> {

		IPAddress m_staticDns;
		IPAddress m_defaultGateway;
		Boolean m_dhcp;
		int m_subnetPrefix;
		IPAddress m_staticIp;

		public IPAddress staticIp {
			get {
				return m_staticIp;
			}
			set {
				if (m_staticIp != value) {
					m_staticIp = value;
					//NotifyPropertyChanged(x => x.staticIp);
				}
			}
		}

		public IPAddress staticDns {
			get {
				return m_staticDns;
			}
			set {
				if (m_staticDns != value) {
					m_staticDns = value;
					//NotifyPropertyChanged(x => x.staticDns);
				}
			}
		}

		public IPAddress defaultGateway {
			get {
				return m_defaultGateway;
			}
			set {
				if (m_defaultGateway != value) {
					m_defaultGateway = value;
					//NotifyPropertyChanged(x => x.defaultGateway);
				}
			}
		}

		public bool dhcp {
			get {
				return m_dhcp;
			}
			set {
				if (m_dhcp != value) {
					m_dhcp = value;
					//NotifyPropertyChanged(x => x.dhcp);
				}
			}
		}

		public int subnetPrefix {
			get {
				return m_subnetPrefix;
			}
			set {
				if (m_subnetPrefix != value) {
					m_subnetPrefix = value;
					//NotifyPropertyChanged(x => x.subnetPrefix);
				}
			}
		}
	}	
}