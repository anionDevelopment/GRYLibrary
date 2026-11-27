<?xml version="1.0" encoding="utf-8"?>
<!--
Test-migration which represents a custom migration of an application: It renames MaximalLengthOfRequestBodies (attribute or element) to RenamedMaximalLengthOfRequestBodies.
It is used to verify that custom migrations are loaded from the embedded resources of the application and executed after the internal migrations of GRYLibrary.
-->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" indent="yes" encoding="utf-8"/>

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="@*[local-name()='MaximalLengthOfRequestBodies']">
    <xsl:attribute name="RenamedMaximalLengthOfRequestBodies" namespace="{namespace-uri()}">
      <xsl:value-of select="."/>
    </xsl:attribute>
  </xsl:template>

  <xsl:template match="*[local-name()='MaximalLengthOfRequestBodies']">
    <xsl:element name="RenamedMaximalLengthOfRequestBodies" namespace="{namespace-uri()}">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
