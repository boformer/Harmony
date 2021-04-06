using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HarmonyLib
{
	[Serializable]
	public class TypeInfo
	{
		[NonSerialized]
		private Type patchType;
		private int typeToken;
		private string moduleGUID;
		private bool isGeneric;
		private TypeInfo[] genericArgumentsInfo;

		public Type PatchType
		{
			get
			{
				if (patchType is null)
				{
					var mdl = AppDomain.CurrentDomain.GetAssemblies()
						.Where(a => !a.FullName.StartsWith("Microsoft.VisualStudio"))
						.SelectMany(a => a.GetLoadedModules())
						.First(m => m.ModuleVersionId.ToString() == moduleGUID);
					if (isGeneric)
					{
						var genericArguments = genericArgumentsInfo.Select(t => t.PatchType).ToArray();
						patchType = mdl.ResolveType(typeToken, genericArguments, null);
					}
					else
						patchType = mdl.ResolveType(typeToken);
				}
				return patchType;
			}
			set
			{
				patchType = value;
				typeToken = patchType.MetadataToken;
				moduleGUID = patchType.Module.ModuleVersionId.ToString();
				isGeneric = patchType.IsGenericType;
				if (isGeneric)
					genericArgumentsInfo = patchType.GetGenericArguments().Select(t => new TypeInfo(t)).ToArray();
			}
		}

		public TypeInfo(Type type)
		{
			PatchType = type;
		}
	}
}
