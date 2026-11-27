<?xml version="1.0" encoding="utf-8"?>
<!--
Renames the properties of DRequestLoggingConfiguration whose names were fixed in GRYLibrary v2.1.7:
MaximalLengthofRequestBodies -> MaximalLengthOfRequestBodies
LoggedHTTPRequeustHeader     -> LoggedHTTPRequestHeader
A property can be serialized as attribute or as element, so both forms are handled. The names are compared by their local name, so the namespaces of the serializer do not matter.
The transformation is idempotent: If a property already has its new name then nothing is changed, and if both names exist then the old one is removed.
-->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" indent="yes" encoding="utf-8"/>

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="@*[local-name()='MaximalLengthofRequestBodies']">
    <xsl:if test="not(../@*[local-name()='MaximalLengthOfRequestBodies'])">
      <xsl:attribute name="MaximalLengthOfRequestBodies" namespace="{namespace-uri()}">
        <xsl:value-of select="."/>
      </xsl:attribute>
    </xsl:if>
  </xsl:template>

  <xsl:template match="@*[local-name()='LoggedHTTPRequeustHeader']">
    <xsl:if test="not(../@*[local-name()='LoggedHTTPRequestHeader'])">
      <xsl:attribute name="LoggedHTTPRequestHeader" namespace="{namespace-uri()}">
        <xsl:value-of select="."/>
      </xsl:attribute>
    </xsl:if>
  </xsl:template>

  <xsl:template match="*[local-name()='MaximalLengthofRequestBodies']">
    <xsl:if test="not(../*[local-name()='MaximalLengthOfRequestBodies'])">
      <xsl:element name="MaximalLengthOfRequestBodies" namespace="{namespace-uri()}">
        <xsl:apply-templates select="@*|node()"/>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template match="*[local-name()='LoggedHTTPRequeustHeader']">
    <xsl:if test="not(../*[local-name()='LoggedHTTPRequestHeader'])">
      <xsl:element name="LoggedHTTPRequestHeader" namespace="{namespace-uri()}">
        <xsl:apply-templates select="@*|node()"/>
      </xsl:element>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>
