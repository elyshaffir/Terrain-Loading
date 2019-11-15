using UnityEngine;

namespace ComputeShading
{
    enum ComputeShaderPropertyType
    {
        Int,
        Float,
        Bool,
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
                    shader.SetInt(intProperty.name, intProperty.value);
                    break;
                case ComputeShaderPropertyType.Float:
                    ComputeShaderFloatProperty floatProperty = (ComputeShaderFloatProperty)this;
                    shader.SetFloat(floatProperty.name, floatProperty.value);
                    break;
                case ComputeShaderPropertyType.Bool:
                    ComputeShaderBoolProperty boolProperty = (ComputeShaderBoolProperty)this;
                    shader.SetBool(boolProperty.name, boolProperty.value);
                    break;
                case ComputeShaderPropertyType.Vector3:
                    ComputeShaderVector3Property vector3Property = (ComputeShaderVector3Property)this;
                    shader.SetVector(vector3Property.name, vector3Property.value);
                    break;
                case ComputeShaderPropertyType.Vector4:
                    ComputeShaderVector4Property vector4Property = (ComputeShaderVector4Property)this;
                    shader.SetVector(vector4Property.name, vector4Property.value);
                    break;
            }
        }

        public abstract ComputeShaderPropertyType GetPropertyType();

        public class ComputeShaderIntProperty : ComputeShaderProperty
        {
            public int value;

            public ComputeShaderIntProperty(string name, int value) : base(name) => this.value = value;

            public override ComputeShaderPropertyType GetPropertyType()
            {
                return ComputeShaderPropertyType.Int;
            }
        }

        public class ComputeShaderFloatProperty : ComputeShaderProperty
        {
            public float value;

            public ComputeShaderFloatProperty(string name, float value) : base(name) => this.value = value;

            public override ComputeShaderPropertyType GetPropertyType()
            {
                return ComputeShaderPropertyType.Float;
            }
        }

        public class ComputeShaderBoolProperty : ComputeShaderProperty
        {
            public bool value;

            public ComputeShaderBoolProperty(string name, bool value) : base(name) => this.value = value;

            public override ComputeShaderPropertyType GetPropertyType()
            {
                return ComputeShaderPropertyType.Bool;
            }
        }

        public class ComputeShaderVector3Property : ComputeShaderProperty
        {
            public Vector3 value;

            public ComputeShaderVector3Property(string name, Vector3 value) : base(name) => this.value = value;

            public override ComputeShaderPropertyType GetPropertyType()
            {
                return ComputeShaderPropertyType.Vector3;
            }
        }

        public class ComputeShaderVector4Property : ComputeShaderProperty
        {
            public Vector4 value;

            public ComputeShaderVector4Property(string name, Vector4 value) : base(name) => this.value = value;

            public override ComputeShaderPropertyType GetPropertyType()
            {
                return ComputeShaderPropertyType.Vector4;
            }
        }
    }
}
