using System;
using System.Collections.Generic;
using Pancake.Editor;


[assembly: RegisterTriTypeProcessor(typeof(GroupNextTypeProcessor), 11000)]

namespace Pancake.Editor
{
    public class GroupNextTypeProcessor : TypeProcessor
    {
        public override void ProcessType(Type type, List<PropertyDefinition> properties)
        {
            PropertyDefinition groupProperty = null;
            GroupAttribute groupAttribute = null;
            TabAttribute tabAttribute = null;

            foreach (var property in properties)
            {
                if (groupProperty != null)
                {
                    if (groupProperty.OwnerType != property.OwnerType || groupProperty.Order + 1000 < property.Order)
                    {
                        groupProperty = null;
                        groupAttribute = null;
                        tabAttribute = null;
                    }
                }

                if (property.Attributes.TryGet(out GroupNextAttribute groupNextAttribute))
                {
                    groupProperty = property;

                    groupAttribute = groupNextAttribute.Path != null ? new GroupAttribute(groupNextAttribute.Path) : null;

                    property.Attributes.TryGet(out tabAttribute);
                }

                if (groupAttribute != null && !property.Attributes.TryGet(out GroupAttribute _))
                {
                    property.GetEditableAttributes().Add(groupAttribute);
                }

                if (tabAttribute != null && !property.Attributes.TryGet(out TabAttribute _))
                {
                    property.GetEditableAttributes().Add(tabAttribute);
                }
            }
        }
    }
}