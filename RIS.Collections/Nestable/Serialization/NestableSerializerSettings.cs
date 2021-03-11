// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Collections.Nestable.Serialization
{
    public class NestableSerializerSettings
    {
        public static NestableSerializerSettings Default { get; }

        public NestableSerializerTargets Targets { get; set; }
        public NestableSerializerCollectionType CollectionType { get; set; }
        public bool SkipReadOnlyFields { get; set; }
        public bool SkipReadOnlyProperties { get; set; }
        public bool FieldsWithAttributeOnly { get; set; }
        public bool PropertiesWithAttributeOnly { get; set; }

        static NestableSerializerSettings()
        {
            Default = new NestableSerializerSettings();
        }

        public NestableSerializerSettings()
        {
            Targets = NestableSerializerTargets.Default;
            CollectionType = NestableSerializerCollectionType.Default;
            SkipReadOnlyFields = true;
            SkipReadOnlyProperties = false;
            FieldsWithAttributeOnly = false;
            PropertiesWithAttributeOnly = false;
        }

        public bool HasTarget(NestableSerializerTargets target)
        {
            return (Targets & target) != 0;
        }

        public NestableCollectionType GetCollectionType()
        {
            return (NestableCollectionType)Enum.Parse(
                typeof(NestableCollectionType),
                CollectionType.ToString());
        }
    }
}
