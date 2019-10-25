using UnityEngine;

namespace ComputeShading
{
    enum ComputeShaderPropertyType
    {
        Int,
        Float,
        Vector3,
        Vector4
    }

    abstract class ComputeShaderProperty
    {
        readonly string name;

        ComputeShaderProperty(string name) => this.name = name;

        public void SetProperty(ComputeShader shader)
        {
            switch (GetPropertyType())
            {
                case ComputeShaderPropertyType.Int:
                    ComputeShaderIntProperty intProperty = (ComputeShaderIntProperty)this;
                    shader.SetInt(intProperty.name, intProperty.Value);
                    break;
                case ComputeShaderPropertyType.Float:
                    ComputeShaderFloatProperty floatProperty = (ComputeShaderFloatProperty)this;
                    shader.SetFloat(floatProperty.name, floatProperty.Value);
                    break;
                case ComputeShaderPropertyType.Vector3:
                    ComputeShaderVector3Property vector3Property = (ComputeShaderVector3Property)this;
                    shader.SetVector(vector3Property.name, vector3Property.Value);
                    break;
                case ComputeShaderPropertyType.Vector4:
                    ComputeShaderVector4Property vector4Property = (ComputeShaderVector4Property)this;
                    shader.SetVector(vector4Property.name, vector4Property.Value);
                    break;
            }
        }

        public abstract ComputeShaderPropertyType GetPropertyType();

        public class ComputeShaderIntProperty : ComputeShaderProperty
        {
            public int Value;
            public ComputeShaderIntProperty(string name, int Value) : base(name) => this.Value = Value;

            public override ComputeShaderPropertyType GetPropertyType()
            {
                return ComputeShaderPropertyType.Int;
            }
        }

        public class ComputeShaderFloatProperty : ComputeShaderProperty
        {
            public float Value;
            public ComputeShaderFloatProperty(string name, float Value) : base(name) => this.Value = Value;

            public override ComputeShaderPropertyType GetPropertyType()
            {
                return ComputeShaderPropertyType.Float;
            }
        }

        public class ComputeShaderVector3Property : ComputeShaderProperty
        {
            public Vector3 Value;
            public ComputeShaderVector3Property(string name, Vector3 Value) : base(name) => this.Value = Value;

            public override ComputeShaderPropertyType GetPropertyType()
            {
                return ComputeShaderPropertyType.Vector3;
            }
        }

        public class ComputeShaderVector4Property : ComputeShaderProperty
        {
            public Vector4 Value;
            public ComputeShaderVector4Property(string name, Vector4 Value) : base(name) => this.Value = Value;

            public override ComputeShaderPropertyType GetPropertyType()
            {
                return ComputeShaderPropertyType.Vector4;
            }
        }
    }
}
