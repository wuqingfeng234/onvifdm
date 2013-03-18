﻿
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Collections.Generic;
using odm.infra;
namespace odm.models {
	

	public interface ICertificateManagementModel:INotifyPropertyChanged{
		string activeCertificateId{get;set;}
		bool clientAuthentication{get;set;}
		LinkedList<string> serverCertificates{get;set;}
		
	}

	public class CertificateManagementModel:IChangeTrackable<ICertificateManagementModel>, ICertificateManagementModel {
		private SimpleChangeTrackable<string> m_activeCertificateId;
		private SimpleChangeTrackable<bool> m_clientAuthentication;
		private ChangeTrackableList<string> m_serverCertificates;
		

		private class OriginAccessor: ICertificateManagementModel {
			private CertificateManagementModel m_model;
			public OriginAccessor(CertificateManagementModel model) {
				m_model = model;
			}
			private PropertyChangedEventHandler cb;
			private object sync = new object();
			event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged {
				add {
					lock(sync){
						cb += value;
					}
				}
				remove {
					lock(sync){
						cb -= value;
					}
				}
			}
			private void NotifyPropertyChanged(string propertyName){
				PropertyChangedEventHandler cb_copy = null;
				lock(sync){
					if(cb!=null){
						cb_copy = cb.Clone() as PropertyChangedEventHandler;
					}
				}
				if (cb_copy != null) {
					cb_copy(this, new PropertyChangedEventArgs(propertyName));
				}
			}
			string ICertificateManagementModel.activeCertificateId {
				get {return m_model.m_activeCertificateId.origin;}
				set {
					if(m_model.m_activeCertificateId.origin != value){
						m_model.m_activeCertificateId.origin = value;
						NotifyPropertyChanged("activeCertificateId");
					}
				}
			}
			bool ICertificateManagementModel.clientAuthentication {
				get {return m_model.m_clientAuthentication.origin;}
				set {
					if(m_model.m_clientAuthentication.origin != value){
						m_model.m_clientAuthentication.origin = value;
						NotifyPropertyChanged("clientAuthentication");
					}
				}
			}
			LinkedList<string> ICertificateManagementModel.serverCertificates {
				get {return m_model.m_serverCertificates.origin;}
				set {
					if(m_model.m_serverCertificates.origin != value){
						m_model.m_serverCertificates.origin = value;
						NotifyPropertyChanged("serverCertificates");
					}
				}
			}
			
		}
		private PropertyChangedEventHandler cb;
		private object sync = new object();
		public event PropertyChangedEventHandler PropertyChanged {
			add {
				lock(sync){
					cb += value;
				}
			}
			remove {
				lock(sync){
					cb -= value;
				}
			}
		}
		private void NotifyPropertyChanged(string propertyName){
			PropertyChangedEventHandler cb_copy = null;
			lock(sync){
				if(cb!=null){
					cb_copy = cb.Clone() as PropertyChangedEventHandler;
				}
			}
			if (cb_copy != null) {
				cb_copy(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		public string activeCertificateId  {
			get {return m_activeCertificateId.current;}
			set {
				if(m_activeCertificateId.current != value) {
					m_activeCertificateId.current = value;
					NotifyPropertyChanged("activeCertificateId");
				}
			}
		}
		
		public bool clientAuthentication  {
			get {return m_clientAuthentication.current;}
			set {
				if(m_clientAuthentication.current != value) {
					m_clientAuthentication.current = value;
					NotifyPropertyChanged("clientAuthentication");
				}
			}
		}
		
		public LinkedList<string> serverCertificates  {
			get {return m_serverCertificates.current;}
			set {
				if(m_serverCertificates.current != value) {
					m_serverCertificates.current = value;
					NotifyPropertyChanged("serverCertificates");
				}
			}
		}
				
		public void AcceptChanges() {
			origin.activeCertificateId = activeCertificateId;
			origin.clientAuthentication = clientAuthentication;
			origin.serverCertificates = serverCertificates;
			
		}

		public void RevertChanges() {
			activeCertificateId = origin.activeCertificateId;
			clientAuthentication = origin.clientAuthentication;
			serverCertificates = origin.serverCertificates;
			
		}

		public bool isModified {
			get {
				if(m_activeCertificateId.isModified)return true;
				if(m_clientAuthentication.isModified)return true;
				if(m_serverCertificates.isModified)return true;
				
				return false;
			}
		}

		public ICertificateManagementModel current {
			get {return this;}
			set {throw new NotImplementedException();}
		}

		public ICertificateManagementModel origin {
			get {return new OriginAccessor(this);}
			set {throw new NotImplementedException();}
		}
	}
}

	