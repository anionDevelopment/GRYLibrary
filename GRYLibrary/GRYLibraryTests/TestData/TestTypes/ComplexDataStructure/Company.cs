using GRYLibrary.Core.AOA;
using GRYLibrary.Core.AOA.SerializeHelper;
using GRYLibrary.Core.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace GRYLibrary.Tests.TestData.TestTypes.ComplexDataStructure
{
    public class Company : IGRYSerializable
    {
        public Employee Manager { get; set; }

        public ISet<Employee> Employees { get; set; } = new HashSet<Employee>();
        public ISet<OrganizationUnit> OrganizationUnits { get; set; } = new HashSet<OrganizationUnit>();
        public ISet<Order> OrderHistory { get; set; } = new HashSet<Order>();
        public string Name { get; set; }
        public ISet<Product> Products { get; set; } = new HashSet<Product>();

        internal static Company GetRandom()
        {
            Company company = new() { Name = "Test-Company" };

            Employee manager = Employee.GetRandom();
            company.Employees.Add(manager);
            company.Manager = manager;
            Employee bossOrg1Personal = Employee.GetRandom();
            company.Employees.Add(bossOrg1Personal);
            Employee bossOrg2Production = Employee.GetRandom();
            company.Employees.Add(bossOrg2Production);
            Employee bossOrg3Accounting = Employee.GetRandom();
            company.Employees.Add(bossOrg3Accounting);
            Employee bossOrg2ProductionProduct1 = Employee.GetRandom();
            company.Employees.Add(bossOrg2ProductionProduct1);
            Employee bossOrg2ProductionProduct2 = Employee.GetRandom();
            company.Employees.Add(bossOrg2ProductionProduct2);
            Employee bossOrg2ProductionProduct3 = Employee.GetRandom();
            company.Employees.Add(bossOrg2ProductionProduct3);
            OrganizationUnit org1Personal = OrganizationUnit.GetRandom(bossOrg1Personal);
            company.OrganizationUnits.Add(org1Personal);
            OrganizationUnit org2Production = OrganizationUnit.GetRandom(bossOrg2Production);
            company.OrganizationUnits.Add(org2Production);
            OrganizationUnit org2ProductionProduct1 = OrganizationUnit.GetRandom(bossOrg2ProductionProduct1);
            company.OrganizationUnits.Add(org2ProductionProduct1);
            OrganizationUnit org2ProductionProduct2 = OrganizationUnit.GetRandom(bossOrg2ProductionProduct2);
            company.OrganizationUnits.Add(org2ProductionProduct2);
            OrganizationUnit org2ProductionProduct3 = OrganizationUnit.GetRandom(bossOrg2ProductionProduct3);
            company.OrganizationUnits.Add(org2ProductionProduct3);
            OrganizationUnit org3Accounting = OrganizationUnit.GetRandom(bossOrg3Accounting);
            company.OrganizationUnits.Add(org3Accounting);
            Employee employee04 = Employee.GetRandom(org2ProductionProduct1);
            company.Employees.Add(employee04);
            Employee employee05 = Employee.GetRandom(org2ProductionProduct1);
            company.Employees.Add(employee05);
            Employee employee06 = Employee.GetRandom(org2ProductionProduct1);
            company.Employees.Add(employee06);
            Employee employee07 = Employee.GetRandom(org2ProductionProduct2);
            company.Employees.Add(employee07);
            Employee employee08 = Employee.GetRandom(org2ProductionProduct2);
            company.Employees.Add(employee08);
            Employee employee09 = Employee.GetRandom(org2ProductionProduct2);
            company.Employees.Add(employee09);
            Employee employee10 = Employee.GetRandom(org2ProductionProduct3);
            company.Employees.Add(employee10);
            Employee employee11 = Employee.GetRandom(org2ProductionProduct3);
            company.Employees.Add(employee11);
            Employee employee12 = Employee.GetRandom(org2ProductionProduct3);
            company.Employees.Add(employee12);
            Employee employee13 = Employee.GetRandom(org1Personal);
            company.Employees.Add(employee13);
            Employee employee14 = Employee.GetRandom(org1Personal);
            company.Employees.Add(employee14);
            Employee employee15 = Employee.GetRandom(org1Personal);
            company.Employees.Add(employee15);
            Employee employee16 = Employee.GetRandom(org3Accounting);
            company.Employees.Add(employee16);
            Employee employee17 = Employee.GetRandom(org3Accounting);
            company.Employees.Add(employee17);
            Employee employee18 = Employee.GetRandom(org3Accounting);
            company.Employees.Add(employee18);

            Product product1 = Product.GetRandom();
            company.Products.Add(product1);
            Product product2 = Product.GetRandom();
            company.Products.Add(product2);
            Product product3 = Product.GetRandom();
            company.Products.Add(product3);

            Enumerable.Range(10, 30).ForEach(index => company.OrderHistory.Add(Order.GetRandom(company.Products)));
            Enumerable.Range(10, 30).ForEach(index => company.OrderHistory.Add(Order.GetRandom(company.Products)));
            Enumerable.Range(10, 30).ForEach(index => company.OrderHistory.Add(Order.GetRandom(company.Products)));

            return company;
        }

        #region Overhead
        public override bool Equals(object @object)
        {
            return Generic.GenericEquals(this, @object);
        }

        public override int GetHashCode()
        {
            return Generic.GenericGetHashCode(this);
        }

        public override string ToString()
        {
            return Generic.GenericToString(this);
        }

        public XmlSchema GetSchema()
        {
            return Generic.GenericGetSchema(this);
        }

        public void ReadXml(XmlReader reader)
        {
            Generic.GenericReadXml(this, reader);
        }

        public void WriteXml(XmlWriter writer)
        {
            Generic.GenericWriteXml(this, writer);
        }

        public ISet<Type> GetExtraTypesWhichAreRequiredForSerialization()
        {
            return new HashSet<Type>();
        }
        #endregion

    }
}