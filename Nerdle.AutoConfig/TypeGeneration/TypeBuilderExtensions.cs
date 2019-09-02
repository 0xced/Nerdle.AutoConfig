using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Nerdle.AutoConfig.TypeGeneration
{
    static class TypeBuilderExtensions
    {
        public static TypeBuilder EmitConstructor(this TypeBuilder typeBuilder, ConstructorInfo constructorInfo)
        {
            var ctor = typeBuilder.DefineConstructor(
                      MethodAttributes.Public |
                      MethodAttributes.SpecialName |
                      MethodAttributes.RTSpecialName,
                      CallingConventions.Standard,
                      Type.EmptyTypes);

            var il = ctor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);                // push "this" on the stack
            il.Emit(OpCodes.Call, constructorInfo);  // call constructor 
            il.Emit(OpCodes.Ret);                    // return

            return typeBuilder;
        }

        public static TypeBuilder EmitProperties(this TypeBuilder typeBuilder, IEnumerable<PropertyInfo> properties)
        {
            return properties.Aggregate(typeBuilder, (tb, prop) => tb.EmitProperty(prop));
        }

        public static TypeBuilder EmitProperty(this TypeBuilder typeBuilder, PropertyInfo property)
        {
            var field = typeBuilder.DefineField("_" + property.Name, property.PropertyType, FieldAttributes.Private);
            var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.PropertyType, null);

            var defaultValueAttribute = property.GetCustomAttributes<DefaultValueAttribute>(true).SingleOrDefault();
            if (defaultValueAttribute != null)
            {
                try
                {
                    var constructor = typeof(DefaultValueAttribute).GetConstructor(new[] { property.PropertyType }) ?? throw new MissingMethodException("DefaultValueAttribute(object) constructor not found");
                    var customAttributeBuilder = new CustomAttributeBuilder(constructor, new[] { defaultValueAttribute.Value });
                    propertyBuilder.SetCustomAttribute(customAttributeBuilder);
                }
                catch (ArgumentException)
                {
                    // exception.Message == "Cannot emit a CustomAttribute with argument of type {type}." (happens when {type} == System.DateTime for example)
                    // So we round-trip the unsupported type to string and use the DefaultValueAttribute(Type, string) constructor.
                    var defaultValue = TypeDescriptor.GetConverter(property.PropertyType).ConvertToInvariantString(defaultValueAttribute.Value);
                    var constructor = typeof(DefaultValueAttribute).GetConstructor(new[] { typeof(Type), typeof(string) }) ?? throw new MissingMethodException("DefaultValueAttribute(Type, string) constructor not found");
                    var customAttributeBuilder = new CustomAttributeBuilder(constructor, new object[] { property.PropertyType, defaultValue });
                    propertyBuilder.SetCustomAttribute(customAttributeBuilder);
                }
            }

            return typeBuilder
                .EmitGetter(propertyBuilder, property, field)
                .EmitSetter(propertyBuilder, property, field);
        }

        static TypeBuilder EmitGetter(this TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, PropertyInfo property, FieldInfo field)
        {
            var getter = typeBuilder.DefineMethod(
                "get_" + property.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                property.PropertyType,
                Type.EmptyTypes);

            var il = getter.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);       // push "this" on the stack
            il.Emit(OpCodes.Ldfld, field);  // load field
            il.Emit(OpCodes.Ret);           // return

            propertyBuilder.SetGetMethod(getter);

            return typeBuilder;
        }

        static TypeBuilder EmitSetter(this TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, PropertyInfo property, FieldInfo field)
        {
            var setter = typeBuilder.DefineMethod(
                "set_" + property.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                null,
                new[] { property.PropertyType });

            var il = setter.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);       // push "this" on the stack
            il.Emit(OpCodes.Ldarg_1);       // push value on the stack
            il.Emit(OpCodes.Stfld, field);  // set field to value
            il.Emit(OpCodes.Ret);           // return

            propertyBuilder.SetSetMethod(setter);

            return typeBuilder;
        }
    }
}
