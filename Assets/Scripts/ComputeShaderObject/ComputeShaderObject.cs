using UnityEngine;

namespace Assets.Scripts.ComputeShaderObject
{
    public class ComputeShaderObject
    {
        public readonly ComputeShader shader;

        private readonly int shaderKernel;

        public ComputeShaderObject(ComputeShader shader, int shaderKernel)
        {
            this.shader = shader;
            this.shaderKernel = shaderKernel;
        }

        public void SetBuffer(string name, ComputeBuffer buffer)
        {
            shader.SetBuffer(shaderKernel, name, buffer);
        }

        public void Dispatch(int threadGroupsX, int threadGroupsY, int threadGroupsZ, ComputeShaderProperty[] properties)
        {
            foreach (ComputeShaderProperty property in properties)
            {
                property.SetProperty(shader);
            }
            shader.Dispatch(shaderKernel, threadGroupsX, threadGroupsY, threadGroupsZ);
        }
    }
}
