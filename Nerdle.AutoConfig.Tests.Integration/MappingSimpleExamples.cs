﻿using System;
using System.IO;
using System.Net;
using FluentAssertions;
using NUnit.Framework;

namespace Nerdle.AutoConfig.Tests.Integration
{
    [TestFixture]
    public class MappingSimpleExamples : EndToEndTest
    {
        [Test]
        public void Mapping_to_an_interface()
        {
            var config = AutoConfig.Map<ISimpleConfiguration>(configFilePath: ConfigFilePath);
            config.Should().BeAssignableTo<ISimpleConfiguration>();
            config.Should().NotBeNull();
            config.MyString.Should().Be("hello");
            config.MyInt.Should().Be(42);
            config.MyDate.Should().Be(new DateTime(1969, 07, 21));
            config.MyBool.Should().BeTrue();
            config.MyNullable.Should().Be(23);
            config.MyEmptyNullable.Should().NotHaveValue();
            config.MyTimeSpan.Should().Be(TimeSpan.FromMinutes(5));
            config.MyProxy.Address.Should().Be(new Uri("http://proxy.example.com:8080"));
            config.MyFileInfo.Name.Should().Be("file");
            config.MyDirectoryInfo.Name.Should().Be("dir");
        }

        [Test]
        public void Mapping_to_a_concrete_class()
        {
            var config = AutoConfig.Map<SimpleConfiguration>(configFilePath: ConfigFilePath);
            config.Should().BeOfType<SimpleConfiguration>();
            config.Should().NotBeNull();
            config.MyString.Should().Be("hello");
            config.MyInt.Should().Be(42);
            config.MyDate.Should().Be(new DateTime(1969, 07, 21));
            config.MyBool.Should().BeTrue();
            config.MyNullable.Should().Be(23);
            config.MyEmptyNullable.Should().NotHaveValue();
            config.MyTimeSpan.Should().Be(TimeSpan.FromMinutes(5));
            config.MyProxy.Address.Should().Be(new Uri("http://proxy.example.com:8080"));
            config.MyFileInfo.Name.Should().Be("file");
            config.MyDirectoryInfo.Name.Should().Be("dir");
        }

        public interface ISimpleConfiguration
        {
            string MyString { get; }
            int MyInt { get; }
            DateTime MyDate { get; }
            bool MyBool { get; }
            int? MyNullable { get; }
            int? MyEmptyNullable { get; }
            TimeSpan MyTimeSpan { get; }
            WebProxy MyProxy { get; }
            FileInfo MyFileInfo { get; }
            DirectoryInfo MyDirectoryInfo { get; }
        }

        public class SimpleConfiguration
        {
            public string MyString { get; set; }
            public int MyInt { get; set; }
            public DateTime MyDate { get; set; }
            public bool MyBool { get; set; }
            public int? MyNullable { get; set; }
            public int? MyEmptyNullable { get; set; }
            public TimeSpan MyTimeSpan { get; set; }
            public WebProxy MyProxy { get; set; }
            public FileInfo MyFileInfo { get; set;  }
            public DirectoryInfo MyDirectoryInfo { get; set;  }
        }
    }
}
