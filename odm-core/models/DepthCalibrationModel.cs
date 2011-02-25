﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml;
using System.Globalization;

using odm.onvif;
using odm.utils;
using onvif.services.media;
using onvif.services.analytics;
using media = onvif.services.media;
using analytics = onvif.services.analytics;
using tt = global::onvif.types;
using System.Xml.Serialization;
using onvif;


namespace odm.models {
	//[Serializable]
	public class Marker {
		//[Serializable]
		//public class Line{
		//    [XmlAttribute]
		//    public int x;
		//    [XmlAttribute]
		//    public int bottom;
		//    [XmlAttribute]
		//    public int top;
		//}
		//[XmlElement]
		public tt::Vector size;

		//[XmlElement]
		public tt::Polyline line1;

		//[XmlElement]
		public tt::Polyline line2;

		public static tt::IntRectangle GetRectFromPolyline(tt::Polyline line) {
			var rect = new tt.IntRectangle();
			rect.x = Math.Min((int)line.Point[0].x, (int)line.Point[1].x);
			rect.y = Math.Min((int)line.Point[0].y, (int)line.Point[1].y);
			rect.width = Math.Abs((int)line.Point[1].x - (int)line.Point[0].x);
			rect.height = Math.Abs((int)line.Point[1].y - (int)line.Point[0].y);
			return rect;
		}

		public static tt::Polyline GetPolylineFromRect(tt::IntRectangle rect) {
			return new tt::Polyline() {
				Point = new tt.Vector[]{
					new tt::Vector(){
						x = rect.x,
			            xSpecified = true,
			            y = rect.y,
			            ySpecified = true
					},
					new tt::Vector(){
						x = rect.x + rect.width,
						xSpecified = true,
						y = rect.y + rect.height,
						ySpecified = true,
					}
				}
			};
		}

		//[XmlIgnore]
		//public tt::IntRectangle rect1 {
		//    get {
		//        return GetRectFromPolyline(line1);
		//    }
		//}

		//[XmlIgnore]
		//public tt::IntRectangle rect2 {
		//    get {
		//        return GetRectFromPolyline(line2);
		//    }
		//}
	}

	public enum MarkerType {
		marker1D,
		marker2D
	}

	public partial class DepthCalibrationModel : ModelBase<DepthCalibrationModel> {
		//ChannelDescription m_channel;
		//public DepthCalibrationModel(ChannelDescription channel) {
		//    m_channel = channel;
		//}
		ProfileToken m_profileToken;
		public DepthCalibrationModel(ProfileToken profileToken) {
			this.m_profileToken = profileToken;
		}

		protected override IEnumerable<IObservable<Object>> LoadImpl(Session session, IObserver<DepthCalibrationModel> observer) {
			AnalyticsObservable analytics = null;
			yield return session.GetAnalyticsClient().Handle(x => analytics = x);
			dbg.Assert(analytics != null);		

			MediaObservable media = null;
			yield return session.GetMediaClient().Handle(x => media = x);
			dbg.Assert(media != null);

			Profile[] profiles = null;
			yield return session.GetProfiles().Handle(x => profiles = x);
			dbg.Assert(profiles != null);

			//var profile = profiles.Where(x => x.token == NvcHelper.GetChannelProfileToken(m_channel.Id)).FirstOrDefault();
			//if (profile == null) {
			//    yield return session.CreateDefaultProfile(m_channel.Id).Handle(x => profile = x);
			//}
			var profile = profiles.Where(x => x.token == m_profileToken).FirstOrDefault();
			dbg.Assert(profile != null);

			yield return session.AddDefaultVideoAnalytics(profile).Idle();
			//yield return session.AddDefaultMetadata(profile).Idle();
			
			//var meta = profile.MetadataConfiguration;
			//if (!meta.AnalyticsSpecified || !meta.Analytics) {
			//    meta.AnalyticsSpecified = true;
			//    meta.Analytics = true;
			//    yield return media.SetMetadataConfiguration(meta, true).Idle();
			//}
			
			VideoAnalyticsConfiguration vac = profile.VideoAnalyticsConfiguration;
			dbg.Assert(vac != null);

			media::Config module = null;
			yield return session.GetVideoAnalyticModule(profile, "SceneCalibrator").Handle(x=>module = x);
			dbg.Assert(module != null);

			var roi = module.Parameters.ElementItem
				.Where(x => x.Name == "roi")
				.Select(x=>x.Any.Deserialize<tt::Polygon>())
				.FirstOrDefault();
			if (roi != null) {
				region = (roi.Point??new tt::Vector[0])
					.Select(p => new Point((int)p.x, (int)p.y))
					.ToList();
			} else {
				region = null;
			}

			m_focalLength.SetBoth(module.GetSimpleItemAsInt("focal_length"));
			matrixFormat = module.GetSimpleItem("matrix_format");
			photosensorPixelSize = module.GetSimpleItemAsFloat("photosensor_pixel_size");
			var use_2d_markers = module.GetSimpleItemAsBoolNullable("use_2d_markers");
			use2DMarkers = use_2d_markers.GetValueOrDefault(false);
			is2DmarkerSupported = use_2d_markers.HasValue;

			Marker m1 = new Marker();
			
			
			if (!use2DMarkers) {
				m1.size = new tt::Vector();
				m1.size.y = module.GetSimpleItemAsInt("marker0_physical_height");
				m1.size.ySpecified = true;
				
				m1.size.x = 0;
				m1.size.xSpecified = true;

				m1.line1 = module.Parameters
					.ElementItem
					.Where(x => x.Name == "marker0_line0")
					.Select(x => x.Any.Deserialize<tt::Polyline>())
					.FirstOrDefault();

				m1.line2 = module.Parameters
					.ElementItem
					.Where(x => x.Name == "marker0_line1")
					.Select(x => x.Any.Deserialize<tt::Polyline>())
					.FirstOrDefault();

			} else {

				m1.size = module.Parameters
					.ElementItem
					.Where(x => x.Name == "marker0_size")
					.Select(x => x.Any.Deserialize<tt::Vector>())
					.FirstOrDefault();

				m1.line1 = module.Parameters
					.ElementItem
					.Where(x => x.Name == "marker0_rect0")
					.Select(x => x.Any.Deserialize<tt::IntRectangle>())
					.Select(x => Marker.GetPolylineFromRect(x))
					.FirstOrDefault();

				m1.line2 = module.Parameters
					.ElementItem
					.Where(x => x.Name == "marker0_rect1")
					.Select(x => x.Any.Deserialize<tt::IntRectangle>())
					.Select(x => Marker.GetPolylineFromRect(x))
					.FirstOrDefault();
			}

			markers = new Marker[] { m1 };
			
			bounds.X = profile.VideoSourceConfiguration.Bounds.x;
			bounds.Y = profile.VideoSourceConfiguration.Bounds.y;
			bounds.Width = profile.VideoSourceConfiguration.Bounds.width;
			bounds.Height = profile.VideoSourceConfiguration.Bounds.height;

			encoderResolution = new Size() {
				Width = profile.VideoEncoderConfiguration.Resolution.Width,
				Height = profile.VideoEncoderConfiguration.Resolution.Height
			};

			NotifyPropertyChanged(x => x.region);
			NotifyPropertyChanged(x => x.focalLength);
			NotifyPropertyChanged(x => x.matrixFormat);
			NotifyPropertyChanged(x => x.photosensorPixelSize);
			NotifyPropertyChanged(x => x.encoderResolution);
			NotifyPropertyChanged(x => x.bounds);

			var streamSetup = new StreamSetup();
			streamSetup.Stream = StreamType.RTPUnicast;
			streamSetup.Transport = new Transport();
			streamSetup.Transport.Protocol = TransportProtocol.UDP;
			streamSetup.Transport.Tunnel = null;
			
			yield return session.GetStreamUri(streamSetup, profile.token).Handle(x => mediaUri = x);
			dbg.Assert(mediaUri != null);
			NotifyPropertyChanged(x => x.mediaUri);

			isModified = true;

			if (observer != null) {
				observer.OnNext(this);
			}
		}
		
		protected override IEnumerable<IObservable<object>> ApplyChangesImpl(Session session, IObserver<DepthCalibrationModel> observer) {

			MediaObservable media = null;
			yield return session.GetMediaClient().Handle(x => media = x);
			dbg.Assert(media != null);

			Profile[] profiles = null;
			yield return session.GetProfiles().Handle(x => profiles = x);
			dbg.Assert(profiles != null);

			//var profile = profiles.Where(x => x.token == NvcHelper.GetChannelProfileToken(m_channel.Id)).FirstOrDefault();
			var profile = profiles.Where(x => x.token == m_profileToken).FirstOrDefault();
			var vac = profile.VideoAnalyticsConfiguration;
			if (vac == null) {
				yield return session.AddDefaultVideoAnalytics(profile).Handle(x=> vac=x);
				profile.VideoAnalyticsConfiguration = vac;
			}

			media::Config module = null;
			yield return session.GetVideoAnalyticModule(profile, "SceneCalibrator").Handle(x => module = x);
			dbg.Assert(module != null);

			var roi = module.Parameters.ElementItem
				.Where(x => x.Name == "roi")
				.FirstOrDefault();
			roi.Any = new tt::Polygon() {
				Point = region.Select(p => new tt::Vector() {
					x = p.X,
					xSpecified = true,
					y = p.Y,
					ySpecified = true
				}).ToArray()
			}.Serialize();

			var m1 = markers[0];

			module.SetSimpleItemAsBool("use_2d_markers", use2DMarkers);
			module.SetSimpleItemAsInt("focal_length", focalLength);
			module.SetSimpleItem("matrix_format", matrixFormat);
			module.SetSimpleItemAsFloat("photosensor_pixel_size", photosensorPixelSize);
			
			foreach (var p in m1.line1.Point) {
				p.xSpecified = true;
				p.ySpecified = true;
			}

			foreach (var p in m1.line2.Point) {
				p.xSpecified = true;
				p.ySpecified = true;
			}

			if (!use2DMarkers) {
				module.SetSimpleItemAsInt("marker0_physical_height", (int)m1.size.y);

				module.Parameters
					.ElementItem
					.Where(x => x.Name == "marker0_line0")
					.FirstOrDefault()
					.Any = m1.line1.Serialize();

				module.Parameters
					.ElementItem
					.Where(x => x.Name == "marker0_line1")
					.FirstOrDefault()
					.Any = m1.line2.Serialize();
			} else {

				module.Parameters
					.ElementItem
					.Where(x => x.Name == "marker0_size")
					.FirstOrDefault()
					.Any = m1.size.Serialize();

				module.Parameters
					.ElementItem
					.Where(x => x.Name == "marker0_rect0")
					.FirstOrDefault()
					.Any = Marker.GetRectFromPolyline(m1.line1).Serialize();
				
				module.Parameters
					.ElementItem
					.Where(x => x.Name == "marker0_rect1")
					.FirstOrDefault()
					.Any = Marker.GetRectFromPolyline(m1.line2).Serialize();
				
			}

			yield return media.SetVideoAnalyticsConfiguration(vac, true).Idle();

			if (observer != null) {
				observer.OnNext(this);
			}

		}

		private ChangeTrackingProperty<int> m_focalLength = new ChangeTrackingProperty<int>();
		public int focalLength {
			get {
				return m_focalLength.current;
			}
			set {
				if (m_focalLength.current != value) {
					m_focalLength.SetCurrent(m_changeSet, value);
					NotifyPropertyChanged(x => x.focalLength);
				}
			}
		}
		
		public string matrixFormat { get; set; }
		public float photosensorPixelSize { get; set; }
		public Marker[] markers { get; set; }
		public bool use2DMarkers{ get; set; }
		public System.Drawing.Rectangle bounds;
		public List<Point> region { get; set; }
		public Size encoderResolution{ get; private set; }

		public bool is2DmarkerSupported { get; set; }

		public string mediaUri {
			get;
			private set;
		}
	}

	public class HeigthMarker {
		int physikalHeigth { get; set; }
		System.Drawing.Rectangle marker { get; set; }
	}
}