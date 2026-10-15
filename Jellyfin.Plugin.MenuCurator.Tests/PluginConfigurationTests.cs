using System.IO;
using System.Text.Json;
using System.Xml.Serialization;
using Jellyfin.Plugin.MenuCurator.Configuration;
using Xunit;

namespace Jellyfin.Plugin.MenuCurator.Tests;

public class PluginConfigurationTests
{
    // The shape the config page posts.
    private const string PostedJson = """
        {"Entries":[
          {"Name":"Moonbase","Visible":true,"DisplayName":"Night mode","MenuIcon":"bedtime","Order":0},
          {"Name":"File Transformation","Visible":false,"DisplayName":null,"MenuIcon":null,"Order":null}
        ]}
        """;

    [Fact]
    public void PostedJson_SurvivesTheXmlStore()
    {
        var posted = JsonSerializer.Deserialize<PluginConfiguration>(PostedJson)!;

        var serializer = new XmlSerializer(typeof(PluginConfiguration));
        using var buffer = new MemoryStream();
        serializer.Serialize(buffer, posted);
        buffer.Position = 0;
        var stored = (PluginConfiguration)serializer.Deserialize(buffer)!;

        Assert.Equal(2, stored.Entries.Length);

        var moonbase = stored.Entries[0];
        Assert.Equal("Moonbase", moonbase.Name);
        Assert.True(moonbase.Visible);
        Assert.Equal("Night mode", moonbase.DisplayName);
        Assert.Equal("bedtime", moonbase.MenuIcon);
        Assert.Equal(0, moonbase.Order);

        var hidden = stored.Entries[1];
        Assert.False(hidden.Visible);
        Assert.Null(hidden.DisplayName);
        Assert.Null(hidden.MenuIcon);
        Assert.Null(hidden.Order);
    }

    [Fact]
    public void Phase2Rule_WithoutNewFields_StillLoads()
    {
        const string Xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <PluginConfiguration xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
              <Entries>
                <MenuEntryRule>
                  <Name>File Transformation</Name>
                  <Visible>false</Visible>
                </MenuEntryRule>
              </Entries>
            </PluginConfiguration>
            """;

        using var reader = new StringReader(Xml);
        var stored = (PluginConfiguration)new XmlSerializer(typeof(PluginConfiguration)).Deserialize(reader)!;

        var rule = Assert.Single(stored.Entries);
        Assert.False(rule.Visible);
        Assert.Null(rule.Order);
    }
}
