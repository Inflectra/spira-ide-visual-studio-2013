using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business
{
	[XmlRoot("list")]
	public class SerializableList<TValue>
		: List<TValue>, IXmlSerializable
	{
		#region IXmlSerializable Members
		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(System.Xml.XmlReader reader)
		{
			XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

			//Turn normalization off..
			if (reader.GetType() == typeof(XmlTextReader))
				((XmlTextReader)reader).Normalization = false;

			bool wasEmpty = reader.IsEmptyElement;
			reader.Read();

			if (wasEmpty)
				return;

			while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
			{
				reader.ReadStartElement("item");

				reader.ReadStartElement("value");
				TValue value = (TValue)valueSerializer.Deserialize(reader);
				reader.ReadEndElement();

				this.Add(value);

				reader.ReadEndElement();
				reader.MoveToContent();
			}
			reader.ReadEndElement();
		}

		public void WriteXml(System.Xml.XmlWriter writer)
		{
			XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

			foreach (TValue value in this)
			{
				writer.WriteStartElement("item");

				writer.WriteStartElement("value");
				valueSerializer.Serialize(writer, value);
				writer.WriteEndElement();

				writer.WriteEndElement();
			}
		}
		#endregion
	}
}
