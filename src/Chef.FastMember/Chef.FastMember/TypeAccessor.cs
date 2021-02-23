using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Chef.FastMember
{
    public class TypeAccessor
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<string, Func<object, object>>> Getters = new ConcurrentDictionary<Type, Dictionary<string, Func<object, object>>>();
        private static readonly ConcurrentDictionary<Type, Dictionary<string, Action<object, object>>> Setters = new ConcurrentDictionary<Type, Dictionary<string, Action<object, object>>>();
        private static readonly ConcurrentDictionary<Type, Dictionary<string, Func<object, object[], object>>> Functions = new ConcurrentDictionary<Type, Dictionary<string, Func<object, object[], object>>>();

        private readonly Dictionary<string, Func<object, object>> getters;
        private readonly Dictionary<string, Action<object, object>> setters;
        private readonly Dictionary<string, Func<object, object[], object>> functions;

        private TypeAccessor(Type type)
        {
            this.getters = Getters.GetOrAdd(type, t => CreateGetters(t));
            this.setters = Setters.GetOrAdd(type, t => CreateSetters(t));
            this.functions = Functions.GetOrAdd(type, t => CreateFunctions(t));
        }

        public object this[object target, string name]
        {
            get
            {
                if (!this.getters.TryGetValue(name, out var getter))
                {
                    throw new ArgumentOutOfRangeException("name");
                }

                return getter(target);
            }

            set
            {
                if (!this.setters.TryGetValue(name, out var setter))
                {
                    throw new ArgumentOutOfRangeException("name");
                }

                setter(target, value);
            }
        }

        public static TypeAccessor Create(Type type)
        {
            return new TypeAccessor(type);
        }

        public static TypeAccessor Create<T>()
        {
            return Create(typeof(T));
        }

        public object Invoke(object instance, string name, params object[] arguments)
        {
            if (!this.functions.TryGetValue(name, out var func))
            {
                throw new ArgumentOutOfRangeException("name");
            }

            return func(instance, arguments);
        }

        // 建立取得屬性及欄位值的委派方法
        private static Dictionary<string, Func<object, object>> CreateGetters(Type type)
        {
            var getters = new Dictionary<string, Func<object, object>>();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var getterMethod = new DynamicMethod(property.Name, typeof(object), new[] { typeof(object) }, type, true);

                var il = getterMethod.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Callvirt, property.GetMethod);
                il.Emit(OpCodes.Box, property.PropertyType);
                il.Emit(OpCodes.Ret);

                getters.Add(property.Name, getterMethod.CreateDelegate(typeof(Func<object, object>)) as Func<object, object>);
            }

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                var getterMethod = new DynamicMethod(field.Name, typeof(object), new[] { typeof(object) }, type, true);

                var il = getterMethod.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, field);
                il.Emit(OpCodes.Box, field.FieldType);
                il.Emit(OpCodes.Ret);

                getters.Add(field.Name, getterMethod.CreateDelegate(typeof(Func<object, object>)) as Func<object, object>);
            }

            return getters;
        }

        // 建立賦予屬性及欄位值的委派方法
        private static Dictionary<string, Action<object, object>> CreateSetters(Type type)
        {
            var setters = new Dictionary<string, Action<object, object>>();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var setterMethod = new DynamicMethod(property.Name, null, new[] { typeof(object), typeof(object) }, type, true);

                var il = setterMethod.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Unbox_Any, property.PropertyType);
                il.Emit(OpCodes.Callvirt, property.SetMethod);
                il.Emit(OpCodes.Ret);

                setters.Add(property.Name, setterMethod.CreateDelegate(typeof(Action<object, object>)) as Action<object, object>);
            }

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                var setterMethod = new DynamicMethod(field.Name, null, new[] { typeof(object), typeof(object) }, type, true);

                var il = setterMethod.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Unbox_Any, field.FieldType);
                il.Emit(OpCodes.Stfld, field);
                il.Emit(OpCodes.Ret);

                setters.Add(field.Name, setterMethod.CreateDelegate(typeof(Action<object, object>)) as Action<object, object>);
            }

            return setters;
        }

        // 建立每個方法的委派方法
        private static Dictionary<string, Func<object, object[], object>> CreateFunctions(Type type)
        {
            var functions = new Dictionary<string, Func<object, object[], object>>();

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var method in methods)
            {
                var invokedMethod = new DynamicMethod(method.Name, typeof(object), new[] { typeof(object), typeof(object[]) }, type, true);

                var il = invokedMethod.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);

                var parameters = method.GetParameters();

                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];

                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldelem_Ref);
                    il.Emit(OpCodes.Unbox_Any, parameter.ParameterType);
                }

                il.Emit(OpCodes.Call, method);

                if (method.ReturnType != typeof(void))
                {
                    il.Emit(OpCodes.Box, method.ReturnType);
                }
                else
                {
                    il.Emit(OpCodes.Ldnull);
                }

                il.Emit(OpCodes.Ret);

                functions.Add(method.Name, invokedMethod.CreateDelegate(typeof(Func<object, object[], object>)) as Func<object, object[], object>);
            }

            return functions;
        }
    }
}