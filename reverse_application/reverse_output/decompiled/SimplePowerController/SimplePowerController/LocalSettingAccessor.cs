using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SimplePowerController;

internal class LocalSettingAccessor
{
	private static string XmlPath;

	private static XElement root;

	public static string DataSaveDir
	{
		get
		{
			return root.Element("DataSaveDir").Value;
		}
		set
		{
			root.Element("DataSaveDir").Value = value;
			root.Save(XmlPath);
		}
	}

	static LocalSettingAccessor()
	{
		XmlPath = AppDomain.CurrentDomain.BaseDirectory + "para.xml";
		if (!File.Exists(XmlPath))
		{
			string content = AppDomain.CurrentDomain.BaseDirectory + "Data";
			root = new XElement("Root", new XElement("Commports", new XElement("PortX", new XAttribute("Port", ""), new XAttribute("BaudRate", "9600")), new XElement("PortY", new XAttribute("Port", ""), new XAttribute("BaudRate", "9600")), new XElement("PortZ", new XAttribute("Port", ""), new XAttribute("BaudRate", "9600"))), new XElement("DataSaveDir", content), new XElement("CoilConstant", new XAttribute("X", "1.0"), new XAttribute("Y", "1.0"), new XAttribute("Z", "1.0")), new XElement("ZeroOffset", new XAttribute("X", "0.0"), new XAttribute("Y", "0.0"), new XAttribute("Z", "0.0")));
			root.Save(XmlPath);
		}
		else
		{
			root = XElement.Load(XmlPath);
		}
	}

	public static void ReadCommports(out string[] ports, out int[] baudRates)
	{
		ports = new string[3];
		baudRates = new int[3];
		IEnumerable<XElement> enumerable = root.Element("Commports").Elements();
		if (enumerable.Count() != 3)
		{
			throw new Exception("配置文件读取错误！");
		}
		int num = 0;
		foreach (XElement item in enumerable)
		{
			ports[num] = item.Attribute("Port").Value;
			baudRates[num] = int.Parse(item.Attribute("BaudRate").Value);
			num++;
		}
	}

	public static void SaveCommports(string[] ports, int[] baudRates)
	{
		IEnumerable<XElement> enumerable = root.Element("Commports").Elements();
		int num = 0;
		foreach (XElement item in enumerable)
		{
			item.Attribute("Port").Value = ports[num];
			item.Attribute("BaudRate").Value = baudRates[num].ToString();
			num++;
		}
		root.Save(XmlPath);
	}

	public static void ReadCoilConstant(out double coilConstantX, out double coilConstantY, out double coilConstantZ)
	{
		XElement xElement = root.Element("CoilConstant");
		coilConstantX = double.Parse(xElement.Attribute("X").Value);
		coilConstantY = double.Parse(xElement.Attribute("Y").Value);
		coilConstantZ = double.Parse(xElement.Attribute("Z").Value);
	}

	public static void SaveCoilConstant(double coilConstantX, double coilConstantY, double coilConstantZ)
	{
		XElement xElement = root.Element("CoilConstant");
		xElement.Attribute("X").Value = coilConstantX.ToString();
		xElement.Attribute("Y").Value = coilConstantY.ToString();
		xElement.Attribute("Z").Value = coilConstantZ.ToString();
		root.Save(XmlPath);
	}

	public static void ReadZeroOffset(out double zeroOffsetX, out double zeroOffsetY, out double zeroOffsetZ)
	{
		XElement xElement = root.Element("ZeroOffset");
		zeroOffsetX = double.Parse(xElement.Attribute("X").Value);
		zeroOffsetY = double.Parse(xElement.Attribute("Y").Value);
		zeroOffsetZ = double.Parse(xElement.Attribute("Z").Value);
	}

	public static void SaveZeroOffset(double zeroOffsetX, double zeroOffsetY, double zeroOffsetZ)
	{
		XElement xElement = root.Element("ZeroOffset");
		xElement.Attribute("X").Value = zeroOffsetX.ToString();
		xElement.Attribute("Y").Value = zeroOffsetY.ToString();
		xElement.Attribute("Z").Value = zeroOffsetZ.ToString();
		root.Save(XmlPath);
	}
}
