﻿using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.FSharp.Control;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Unity;
using odm.infra;
using odm.ui.controls;
using onvif.services;
using utils;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace odm.ui.activities {

	

	public partial class NetworkSettingsView : UserControl, IDisposable {

		#region Activity definition
		public static FSharpAsync<Result> Show(IUnityContainer container, Model model) {
			return container.StartViewActivity<Result>(context => {
				var view = new NetworkSettingsView(model, context);
				var presenter = container.Resolve<IViewPresenter>();
				presenter.ShowView(view);
			});
		}
		#endregion

		public ICommand CancelCommand { get; private set; }

		public LinkButtonsStrings Titles { get { return LinkButtonsStrings.instance; } }
		public LocalButtons ButtonsLocales { get { return LocalButtons.instance; } }
		public LocalNetworkSettings Strings { get { return LocalNetworkSettings.instance; } }

		Model model;
		private void Init(Model model) {
			this.DataContext = model;
			this.model = model;

			OnCompleted+=new Action(()=>{
				//free resources
			});

			var applyCmd = new DelegateCommand(
				() => {
					if (!model.dhcp) {
						model.useNtpFromDhcp = false;
						model.useDnsFromDhcp = false;
					}
					GetProtocolData();
					Success(new Result.Apply(model));
				},
				() => true
			);
			ApplyCommand = applyCmd;

			var cancelCmd = new DelegateCommand(
				() => {
					model.RevertChanges();
					FillProtocolsData(model);
				},
				() => true
			);
			CancelCommand = cancelCmd;

			InitializeComponent();

			Localization();
			BindData(model);
		}

		void Localization() {
			dhcpCaption.CreateBinding(TextBlock.TextProperty, Strings, x => x.dhcp);
			dnsCaption.CreateBinding(TextBlock.TextProperty, Strings, x => x.dns);
			//dnsFromDhcpCaption.CreateBinding(TextBlock.TextProperty, Strings, x => x.dnsFromDhcp);
			gatewayCaption.CreateBinding(TextBlock.TextProperty, Strings, x => x.gateway);
			ipAddressCaption.CreateBinding(TextBlock.TextProperty, Strings, x => x.ipAddress);
			ipMaskCaption.CreateBinding(TextBlock.TextProperty, Strings, x => x.ipMask);
			ntpCaption.CreateBinding(TextBlock.TextProperty, Strings, x => x.ntp);
			//ntpFromDhcpCaption.CreateBinding(TextBlock.TextProperty, Strings, x => x.ntpFromDhcp);
			applyButton.CreateBinding(Button.ContentProperty, ButtonsLocales, x => x.apply);
			cancelButton.CreateBinding(Button.ContentProperty, ButtonsLocales, x => x.cancel);
			zeroCaption.CreateBinding(TextBlock.TextProperty, Strings, x => x.zeroCaption);
			//captionPortsHttp.CreateBinding(TextBlock.TextProperty, Strings, s => s.ports);
			//captionPortsHttps.CreateBinding(TextBlock.TextProperty, Strings, s => s.ports);
			//captionPortsRtsp.CreateBinding(TextBlock.TextProperty, Strings, s => s.ports);
		}

		private static Tuple<bool, string> GetProtocolPorts(NetworkProtocol[] protocols, NetworkProtocolType protocolType) {
			var protocolName = protocolType.ToString();
			if (protocols == null) {
				return Tuple.Create(false, "");
			}
			var enabled = false;
			var ports = new List<int>();
			foreach (var protocol in protocols.Where(p => p != null && String.Compare(p.Name, protocolName, true) == 0)) {
				if (protocol.Enabled && !enabled) {
					enabled = true;
					ports.Clear();
				}
				if (protocol.Port != null) {
					ports.AddRange(protocol.Port);
				}
			}
			return Tuple.Create(enabled, String.Join("; ", ports.Select(p => p.ToString())));
		}

		void FillProtocolsData(Model model) {

			var httpPorts = GetProtocolPorts(model.netProtocols, NetworkProtocolType.HTTP);
			valueEnableHttp.SelectedValue = httpPorts.Item1;
			valuePortsHttp.Text = httpPorts.Item2;

			var httpsPorts = GetProtocolPorts(model.netProtocols, NetworkProtocolType.HTTPS);
			valueEnableHttps.SelectedValue = httpsPorts.Item1;
			valuePortsHttps.Text = httpsPorts.Item2;
			
			var rtspPorts = GetProtocolPorts(model.netProtocols, NetworkProtocolType.RTSP);
			valueEnableRtsp.SelectedValue = rtspPorts.Item1;
			valuePortsRtsp.Text = rtspPorts.Item2;
			
		}
		
		private static NetworkProtocol CreateNetworkProtocol(NetworkProtocolType protocolType, string ports, bool enabled){
			return new NetworkProtocol(){
				Name = protocolType.ToString(),
				Port = ports == null ? null : ports.Split(new []{';', ' ', ','}, StringSplitOptions.RemoveEmptyEntries).Select(p=>Int32.Parse(p)).ToArray(),
				Enabled = enabled
			};
		}
		
		void GetProtocolData() {
			model.netProtocols = new NetworkProtocol[] { 
				CreateNetworkProtocol(NetworkProtocolType.HTTP, valuePortsHttp.Text, (bool)valueEnableHttp.SelectedValue),
				CreateNetworkProtocol(NetworkProtocolType.HTTPS, valuePortsHttps.Text, (bool)valueEnableHttps.SelectedValue),
				CreateNetworkProtocol(NetworkProtocolType.RTSP, valuePortsRtsp.Text, (bool)valueEnableRtsp.SelectedValue)
			};
		}

		void BindData(Model model) {
			if (!model.zeroConfSupported) {
				zeroValue.IsEnabled = false;
				zeroIp.Text = "not supported";
			} else {
				zeroValue.CreateBinding(CheckBox.IsCheckedProperty, model,
					x => x.zeroConfEnabled,
					(m, v) => {
						m.zeroConfEnabled = v;
					});
				if (!String.IsNullOrWhiteSpace(model.zeroConfIp)) {
					zeroIp.Text = model.zeroConfIp;
				} else {
					zeroIp.Text = "none";
				}
				
			}

			FillProtocolsData(model);

			dhcpValue.CreateBinding(ComboBox.SelectedValueProperty, model, m => m.dhcp, (m, v) => m.dhcp = v);

			ipAddressValue.CreateBinding(TextBox.IsReadOnlyProperty, model, x => x.dhcp);
			ipAddressValue.CreateBinding(TextBox.TextProperty, model, x => x.ip, (m, v) => m.ip = v);

			ipMaskValue.CreateBinding(TextBox.IsReadOnlyProperty, model, x => x.dhcp);
			ipMaskValue.CreateBinding(TextBox.TextProperty, model, x => x.subnet, (m, v) => m.subnet = v);

			gatewayValue.CreateBinding(TextBox.IsReadOnlyProperty, model, x => x.dhcp);
			gatewayValue.CreateBinding(TextBox.TextProperty, model, x => x.gateway, (m, v) => m.gateway = v);

			hostValue.CreateBinding(TextBox.IsReadOnlyProperty, model, m => m.useHostFromDhcp && model.dhcp);
			hostValue.CreateBinding(TextBox.TextProperty, model, m => m.host, (m, v) => m.host = v);
			
			hostFromDhcpValue.CreateBinding(ComboBox.IsEnabledProperty, model, x => x.dhcp);
			hostFromDhcpValue.CreateBinding(ComboBox.SelectedValueProperty, model, m => m.useHostFromDhcp && model.dhcp, (m, v) => m.useHostFromDhcp = v);

			dnsValue.CreateBinding(TextBox.IsReadOnlyProperty, model, m => m.useDnsFromDhcp && model.dhcp);
			dnsValue.CreateBinding(TextBox.TextProperty, model, x => x.dns, (m, v) => m.dns = v);

			dnsFromDhcpValue.CreateBinding(ComboBox.IsEnabledProperty, model, x => x.dhcp);
			dnsFromDhcpValue.CreateBinding(ComboBox.SelectedValueProperty, model, x => x.useDnsFromDhcp && model.dhcp, (m, v) => m.useDnsFromDhcp = v);

			ntpValue.CreateBinding(TextBox.IsReadOnlyProperty, model, m => m.useNtpFromDhcp && model.dhcp);
			ntpValue.CreateBinding(TextBox.TextProperty, model, x => x.ntpServers, (m, v) => m.ntpServers = v);

			ntpFromDhcpValue.CreateBinding(ComboBox.IsEnabledProperty, model, x => x.dhcp);
			ntpFromDhcpValue.CreateBinding(ComboBox.SelectedValueProperty, model, x => x.useNtpFromDhcp && model.dhcp, (m, v) => m.useNtpFromDhcp = v);

			valueEnableHttp.SelectionChanged += (s, a) => {
				if (a.AddedItems.Count > 0) {
					var sel = a.AddedItems[0] as bool?;
					valuePortsHttp.IsReadOnly = sel != true;
				}
			};

			valueEnableHttps.SelectionChanged += (s, a) => {
				if (a.AddedItems.Count > 0) {
					var sel = a.AddedItems[0] as bool?;
					valuePortsHttps.IsReadOnly = sel != true;
				} 
			};
			valueEnableRtsp.SelectionChanged += (s, a) => {
				if (a.AddedItems.Count > 0) {
					var sel = a.AddedItems[0] as bool?;
					valuePortsRtsp.IsReadOnly = sel != true;
				} 
			};

		}

		public void Dispose() {
			Cancel();
		}
	}
}
