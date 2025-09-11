using IntField = HisaCat.SimpleCrypto.FastCryptoField.IntField;
using FloatField = HisaCat.SimpleCrypto.FastCryptoField.FloatField;
using DoubleField = HisaCat.SimpleCrypto.FastCryptoField.DoubleField;
using BoolField = HisaCat.SimpleCrypto.FastCryptoField.BoolField;
using StringField = HisaCat.SimpleCrypto.FastCryptoField.StringField;

namespace HisaCat.HUE.DataBindEx
{
    public static class DataBindSimpleCrypto
    {
        public abstract class CryptoPropertyBase<T> : Slash.Unity.DataBind.Core.Data.Property
        {
            public abstract new T Value { get; set; }
        }

        public class CryptoIntProperty : CryptoPropertyBase<int>
        {
            private readonly IntField cryptoField = null;
            public CryptoIntProperty() => this.cryptoField = new(default);
            public CryptoIntProperty(int value) => this.cryptoField = new(value);

            public override int Value
            {
                get => this.cryptoField.Value;
                set
                {
                    var changed = !Equals(this.Value, value);
                    if (!changed) return;

                    this.cryptoField.Value = value;

                    this.OnValueChanged();
                }
            }
        }
        public class CryptoFloatProperty : CryptoPropertyBase<float>
        {
            private readonly FloatField cryptoField = null;
            public CryptoFloatProperty() => this.cryptoField = new(default);
            public CryptoFloatProperty(float value) => this.cryptoField = new(value);
            public override float Value
            {
                get => this.cryptoField.Value;
                set
                {
                    var changed = !Equals(this.Value, value);
                    if (!changed) return;

                    this.cryptoField.Value = value;

                    this.OnValueChanged();
                }
            }
        }
        public class CryptoDoubleProperty : CryptoPropertyBase<double>
        {
            private readonly DoubleField cryptoField = null;
            public CryptoDoubleProperty() => this.cryptoField = new(default);
            public CryptoDoubleProperty(double value) => this.cryptoField = new(value);
            public override double Value
            {
                get => this.cryptoField.Value;
                set
                {
                    var changed = !Equals(this.Value, value);
                    if (!changed) return;

                    this.cryptoField.Value = value;

                    this.OnValueChanged();
                }
            }
        }
        public class CryptoBoolProperty : CryptoPropertyBase<bool>
        {
            private readonly BoolField cryptoField = null;
            public CryptoBoolProperty() => this.cryptoField = new(default);
            public CryptoBoolProperty(bool value) => this.cryptoField = new(value);
            public override bool Value
            {
                get => this.cryptoField.Value;
                set
                {
                    var changed = !Equals(this.Value, value);
                    if (!changed) return;

                    this.cryptoField.Value = value;

                    this.OnValueChanged();
                }
            }
        }
        public class CryptoStringProperty : CryptoPropertyBase<string>
        {
            private readonly StringField cryptoField = null;
            public CryptoStringProperty() => this.cryptoField = new(default);
            public CryptoStringProperty(string value) => this.cryptoField = new(value);
            public override string Value
            {
                get => this.cryptoField.Value;
                set
                {
                    var changed = !Equals(this.Value, value);
                    if (!changed) return;

                    this.cryptoField.Value = value;

                    this.OnValueChanged();
                }
            }
        }
    }
}
