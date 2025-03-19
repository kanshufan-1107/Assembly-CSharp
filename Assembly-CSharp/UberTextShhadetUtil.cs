using UnityEngine;

public class UberTextShhadetUtil : IUberTextShaderUtil
{
	public Shader FindShader(string name)
	{
		return ShaderUtils.FindShader(name);
	}
}
