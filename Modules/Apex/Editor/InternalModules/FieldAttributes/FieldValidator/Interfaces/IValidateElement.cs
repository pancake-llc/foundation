namespace Pancake.ApexEditor
{
    public interface IValidateElement
    {
        /// <summary>
        /// Called every inspector update time before drawing property.
        /// </summary>
        /// <param name="element">Serialized element with ValidatorAttribute.</param>
        void Validate(SerializedField element);
    }
}