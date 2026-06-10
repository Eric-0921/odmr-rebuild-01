using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace SimplePowerController.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("SimplePowerController.Properties.Resources", typeof(Resources).Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static Bitmap 保存 => (Bitmap)ResourceManager.GetObject("保存", resourceCulture);

	internal static Bitmap 断开连接 => (Bitmap)ResourceManager.GetObject("断开连接", resourceCulture);

	internal static Bitmap 灰灯 => (Bitmap)ResourceManager.GetObject("灰灯", resourceCulture);

	internal static Bitmap 红灯 => (Bitmap)ResourceManager.GetObject("红灯", resourceCulture);

	internal static Bitmap 绿灯 => (Bitmap)ResourceManager.GetObject("绿灯", resourceCulture);

	internal static Bitmap 连接 => (Bitmap)ResourceManager.GetObject("连接", resourceCulture);

	internal static Bitmap 透明灯 => (Bitmap)ResourceManager.GetObject("透明灯", resourceCulture);

	internal Resources()
	{
	}
}
