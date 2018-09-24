using System;
using System.Xml.Linq;
using FluentAssertions;
using Nerdle.AutoConfig.Mapping.Mappers;
using NUnit.Framework;

namespace Nerdle.AutoConfig.Tests.Integration
{
    [TestFixture]
    class ConfiguringTheMappingStrategy : EndToEndTest
    {
        [Test]
        public void Configuring_the_mapping()
        {
            AutoConfig.WhenMapping<ICustomConfiguration>(
                mapper =>
                {
                    mapper.UseMatchingCase();
                    mapper.Map(x => x.Foo).From("foofoo");
                    mapper.Map(x => x.Bar).Optional();
                    mapper.Map(x => x.Baz).OptionalWithDefault("baz");
                    mapper.Map(x => x.Qux).Using<CustomMapper>();
                });

            var config = AutoConfig.Map<ICustomConfiguration>(configFilePath: ConfigFilePath);

            config.Foo.Should().Be("fooooo");
            config.Bar.Should().BeNull();
            config.Baz.Should().Be("baz");
            config.Qux.Should().Be("From custom mapper");
        }

        [Test]
        public void Progressively_configuring_the_mapping()
        {
            AutoConfig.WhenMapping<ICustomConfiguration>(
                mapper =>
                {
                    mapper.UseMatchingCase();
                });

            AutoConfig.WhenMapping<ICustomConfiguration>(
               mapper =>
               {
                   mapper.Map(x => x.Bar).From("barbar");
                   mapper.UseCamelCase();
               });

            AutoConfig.WhenMapping<ICustomConfiguration>(
               mapper =>
               {
                   mapper.Map(x => x.Bar).Optional();
                   mapper.UseCamelCase();
                   mapper.Map(x => x.Foo).From("foofoo");
               });

            AutoConfig.WhenMapping<ICustomConfiguration>(
               mapper =>
               {
                   mapper.Map(x => x.Foo).From("foofoo");
                   mapper.Map(x => x.Baz).OptionalWithDefault("baz");
                   mapper.Map(x => x.Qux).Using<CustomMapper>();
                   mapper.UseMatchingCase();
               });

            var config = AutoConfig.Map<ICustomConfiguration>(configFilePath: ConfigFilePath);

            config.Foo.Should().Be("fooooo");
            config.Bar.Should().BeNull();
            config.Baz.Should().Be("baz");
            config.Qux.Should().Be("From custom mapper");
        }

        [Test]
        public void Configuring_the_mapping_of_inherited_interfaces()
        {
            AutoConfig.WhenMapping<IBaseConfiguration>(mapper =>
            {
                mapper.Map(x => x.MyString).OptionalWithDefault("base");
                mapper.Map(x => x.MyInt).OptionalWithDefault(42);
            });
            AutoConfig.WhenMapping<IDerivedConfiguration>(mapper =>
            {
                mapper.Map(x => x.MyString).OptionalWithDefault("derived");
            });
            
            var baseConfig = AutoConfig.Map<IBaseConfiguration>(configFilePath: ConfigFilePath);
            baseConfig.MyString.Should().Be("base");
            baseConfig.MyInt.Should().Be(42);
            
            var derivedConfig = AutoConfig.Map<IDerivedConfiguration>(configFilePath: ConfigFilePath);
            derivedConfig.MyString.Should().Be("derived");
            derivedConfig.MyInt.Should().Be(42);
        }

        [Test]
        public void Configuring_the_mapping_of_inherited_classes()
        {
            AutoConfig.WhenMapping<BaseConfiguration>(mapper =>
            {
                mapper.Map(x => x.MyString).OptionalWithDefault("base");
                mapper.Map(x => x.MyInt).OptionalWithDefault(42);
            });
            AutoConfig.WhenMapping<DerivedConfiguration>(mapper =>
            {
                mapper.Map(x => x.MyString).OptionalWithDefault("derived");
            });
            
            var baseConfig = AutoConfig.Map<BaseConfiguration>(configFilePath: ConfigFilePath);
            baseConfig.MyString.Should().Be("base");
            baseConfig.MyInt.Should().Be(42);
            
            var derivedConfig = AutoConfig.Map<DerivedConfiguration>(configFilePath: ConfigFilePath);
            derivedConfig.MyString.Should().Be("derived");
            derivedConfig.MyInt.Should().Be(42);
        }
    }

    public interface ICustomConfiguration
    {
        string Foo { get; }
        string Bar { get; }
        string Baz { get; }
        string Qux { get; }
    }

    public interface IBaseConfiguration
    {
        string MyString { get; }
        int MyInt { get; }
    }

    public class BaseConfiguration
    {
        public string MyString { get; set; }
        public int MyInt { get; set; }
    }
    
    public interface IDerivedConfiguration : IBaseConfiguration
    {
        new string MyString { get; }
    }

    public class DerivedConfiguration : BaseConfiguration
    {
        public new string MyString { get; set; }
    }

    class CustomMapper : IMapper
    {
        public object Map(XElement element, Type type)
        {
            return "From custom mapper";
        }
    }
}