// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using RIS.Extensions;

namespace RIS.Localization
{
    public abstract class LocalizedListBase : INotifyPropertyChanged
    {
        public static readonly string StaticPropertyChangedEventName;



        public event PropertyChangedEventHandler PropertyChanged;



        private static readonly ReadOnlyCollection<Type> Types;
        private static readonly ReadOnlyDictionary<Type, LocalizedListBase> Instances;
        private static readonly ReadOnlyDictionary<Type, FieldInfo> StaticPropertyChangedHandlerFields;



        private readonly Type _type;
        private readonly LocalizationFactory _localizationFactory;
        private readonly ReadOnlyDictionary<string, LocalizedProperty> _propertyMappings;
        private readonly ReadOnlyDictionary<string, LocalizedProperty> _propertyStaticMappings;
        private readonly bool _notifyPerProperty;
        private readonly EventHandler<LocalizationEventArgs> _localizationUpdatedHandler;



        static LocalizedListBase()
        {
            StaticPropertyChangedEventName =
                "StaticPropertyChanged";
            Types =
                GetTypes();
            StaticPropertyChangedHandlerFields =
                GetStaticPropertyChangedHandlerFields(Types);
            Instances =
                CreateInstances(Types);
        }

        protected LocalizedListBase(
            LocalizationFactory factory,
            bool notifyPerProperty = false)
        {
            _type =
                GetType();
            _localizationFactory =
                factory;
            _propertyMappings =
                GetPropertyMappings();
            _propertyStaticMappings =
                GetPropertyStaticMappings();
            _notifyPerProperty =
                notifyPerProperty;
            _localizationUpdatedHandler =
                GetLocalizationUpdatedHandler();

            _localizationUpdatedHandler.Invoke(
                _localizationFactory,
                new LocalizationEventArgs(
                    _localizationFactory));

            _localizationFactory.LocalizationUpdated +=
                _localizationUpdatedHandler;
        }

        ~LocalizedListBase()
        {
            _localizationFactory.LocalizationUpdated -=
                _localizationUpdatedHandler;
        }



        private ReadOnlyDictionary<string, LocalizedProperty> GetPropertyMappings()
        {
            return GetPropertyMappingsInternal(
                BindingFlags.Instance);
        }
        private ReadOnlyDictionary<string, LocalizedProperty> GetPropertyStaticMappings()
        {
            return GetPropertyMappingsInternal(
                BindingFlags.Static);
        }
        private ReadOnlyDictionary<string, LocalizedProperty> GetPropertyMappingsInternal(
            BindingFlags additionalFlags)
        {
            var propertyMappings = new Dictionary<string, LocalizedProperty>();

            foreach (var property in GetType().GetProperties(additionalFlags
                                                             | BindingFlags.Public
                                                             | BindingFlags.GetProperty
                                                             | BindingFlags.SetProperty))
            {
                if (!Attribute.IsDefined(property, typeof(LocalizationKeyAttribute)))
                    continue;

                var localizationKey = property
                    .GetCustomAttribute<LocalizationKeyAttribute>()?
                    .Key;

                if (localizationKey == null)
                    continue;

                object nonLocalizedValue = null;

                if (Attribute.IsDefined(property, typeof(NonLocalizedValueAttribute)))
                {
                    nonLocalizedValue = property
                        .GetCustomAttribute<NonLocalizedValueAttribute>()?
                        .Value;
                }

                var declaringProperty = property
                    .GetDeclaring();
                var localizedProperty = new LocalizedProperty(
                    this,
                    declaringProperty,
                    localizationKey,
                    nonLocalizedValue);

                propertyMappings.Add(
                    localizationKey,
                    localizedProperty);
            }

            return new ReadOnlyDictionary<string, LocalizedProperty>(
                propertyMappings);
        }

        private EventHandler<LocalizationEventArgs> GetLocalizationUpdatedHandler()
        {
            if (_notifyPerProperty)
                return OnLocalizationUpdated_PerProperty;

            return OnLocalizationUpdated_AllProperty;
        }


        private void UpdateValue(
            LocalizedProperty property)
        {
            var key = property.LocalizationKey;
            var defaultValue = property.NonLocalizedValue;
            string value;

            if (defaultValue == null)
            {
                value = _localizationFactory
                    .GetLocalized(key);
            }
            else
            {
                if (!_localizationFactory.TryGetLocalized(key, out value))
                    value = defaultValue.ToString();
            }

            property.SetValue(
                value);
        }


        private void UpdateProperties_PerProperty()
        {
            foreach (var property in _propertyMappings.Values)
            {
                UpdateValue(
                    property);

                OnPropertyChanged(
                    property.Name);
            }
        }

        private void UpdateProperties_AllProperty()
        {
            foreach (var property in _propertyMappings.Values)
            {
                UpdateValue(
                    property);
            }

            OnPropertyChanged(
                string.Empty);
        }

        private void UpdateStaticProperties()
        {
            foreach (var property in _propertyStaticMappings.Values)
            {
                UpdateValue(
                    property);

                OnStaticPropertyChanged(
                    this,
                    property.Name);
            }
        }



        private void OnLocalizationUpdated_PerProperty(object sender,
            LocalizationEventArgs e)
        {
            UpdateProperties_PerProperty();
            UpdateStaticProperties();
        }

        private void OnLocalizationUpdated_AllProperty(object sender,
            LocalizationEventArgs e)
        {
            UpdateProperties_AllProperty();
            UpdateStaticProperties();
        }



        protected void OnPropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }



        private static ReadOnlyCollection<Type> GetTypes()
        {
            var types = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var targetTypes = assembly
                    .GetTypes()
                    .Where(type =>
                        type.IsClass
                        && typeof(LocalizedListBase)
                            .IsAssignableFrom(type)
                        && type != typeof(LocalizedListBase));

                types.AddRange(
                    targetTypes);
            }

            return new ReadOnlyCollection<Type>(
                types);
        }

        private static ReadOnlyDictionary<Type, LocalizedListBase> CreateInstances(
            ReadOnlyCollection<Type> types)
        {
            var instances = new Dictionary<Type, LocalizedListBase>();

            foreach (var type in types)
            {
                object instance;

                try
                {
                    instance = Activator
                        .CreateInstance(
                            type, true);
                }
                catch (Exception)
                {
                    continue;
                }

                if (instance == null)
                    continue;

                instances.Add(
                    type,
                    (LocalizedListBase)instance);
            }

            return new ReadOnlyDictionary<Type, LocalizedListBase>(
                instances);
        }

        private static ReadOnlyDictionary<Type, FieldInfo> GetStaticPropertyChangedHandlerFields(
            ReadOnlyCollection<Type> types)
        {
            var handlerFields = new Dictionary<Type, FieldInfo>();

            foreach (var type in types)
            {
                var handlerField = type.GetField(
                    StaticPropertyChangedEventName,
                    BindingFlags.Static
                    | BindingFlags.NonPublic);

                handlerFields.Add(
                    type,
                    handlerField);
            }

            return new ReadOnlyDictionary<Type, FieldInfo>(
                handlerFields);
        }



        public static T GetInstance<T>()
            where T : LocalizedListBase
        {
            return (T)Instances[typeof(T)];
        }



        protected static void OnStaticPropertyChanged(
            LocalizedListBase sender,
            [CallerMemberName] string propertyName = null)
        {
            OnStaticPropertyChangedInternal(
                sender, sender._type,
                propertyName);
        }
        private static void OnStaticPropertyChangedInternal(
            LocalizedListBase sender, Type senderType,
            string propertyName = null)
        {
            var handlerField =
                StaticPropertyChangedHandlerFields[senderType];
            var handler = (Delegate)handlerField?
                .GetValue(null);

            if (handler == null)
                return;

            _ = handler.DynamicInvoke(
                sender,
                new PropertyChangedEventArgs(propertyName));

            //foreach (var handlerDelegate in (MulticastDelegate)handler.GetInvocationList())
            //{
            //    _ = handlerDelegate.Method.Invoke(
            //        handlerDelegate.Target,
            //        new object[]
            //        {
            //            sender,
            //            new PropertyChangedEventArgs(propertyName)
            //        });
            //}
        }
    }
}
