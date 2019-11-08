using System.Collections.Generic;
using UnityEngine;

namespace ComputeShading
{
    abstract class ComputeShaderObject
    {
        readonly ComputeShader shader;
        readonly int shaderKernel;
        readonly List<ComputeBuffer> usedBuffers;

        protected ComputeShaderObject(ComputeShader shader, int shaderKernel)
        {
            this.shader = shader;
            usedBuffers = new List<ComputeBuffer>();
            this.shaderKernel = shaderKernel;
        }

        protected abstract ComputeShaderProperty[] GetProperties();
        public abstract void SetBuffers();

        protected void AddBuffer(ComputeBuffer buffer)
        {
            usedBuffers.Add(buffer);
        }

        protected void SetBuffer(string name, ComputeBuffer buffer, bool release = true)
        {
            shader.SetBuffer(shaderKernel, name, buffer);
            if (release)
            {
                AddBuffer(buffer);
            }
        }

        public abstract void Dispatch();

        protected void Dispatch(int threadGroupsX, int threadGroupsY, int threadGroupsZ, ComputeShaderProperty[] properties)
        {
            foreach (ComputeShaderProperty property in properties)
            {
                property.SetProperty(shader);
            }
            shader.Dispatch(shaderKernel, threadGroupsX, threadGroupsY, threadGroupsZ);
        }

        public abstract void GetData();

        public void Release()
        {
            foreach (ComputeBuffer usedBuffer in usedBuffers)
            {
                usedBuffer.Release();
            }
        }
    }
}
