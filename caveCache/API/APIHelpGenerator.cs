using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using System.IO;

namespace caveCache.API
{
    class APIHelpGenerator
    {
        string _html;
        ExampleValueFactory _factory;

        public APIHelpGenerator()
        {
            _factory = new ExampleValueFactory();

            // get all request types
            var allTypes = this.GetType().Assembly.GetTypes();
            var requests =
                from t in allTypes
                where t.IsSubclassOf(typeof(Request))
                select t;

            var pairs =
                from request in requests
                join response in allTypes on GetResponseName(request.Name) equals response.Name
                select new { request, response };

            XElement root = new XElement("html");
            XElement header = new XElement("header");
            XElement body = new XElement("body");
            root.Add(header);

            string css;
            var assembly = this.GetType().Assembly;
            using (var fin = assembly.GetManifestResourceStream("caveCache.API.help.css"))
            using (var reader = new StreamReader(fin, Encoding.UTF8))
                css = reader.ReadToEnd();

            header.IncludeStyle(css);

            root.Add(body);

            var container = root.Div().Css("container");

            container.H(1, "Cave Cache API Help");

            foreach (var p in pairs.OrderBy( a=>a.request.Name))
            {
                if (p.request.GetCustomAttribute(typeof(HelpIgnoreAttribute), false) != null)
                    continue;

                var command = container.Div().Css("command");

                command.H(3, p.request.Name);

                var requestAttr = p.request.GetCustomAttribute(typeof(RequestAttribute)) as RequestAttribute;
                if (null != requestAttr)
                    command.Div(requestAttr.Description);                

                BuildClassHelp(command, p.request);
                command.H(4, "Example:");
                command.Pre( _factory.GetExampleValue(p.request) ).Css("code");

                command.H(3, p.response.Name);
                BuildClassHelp(command, p.response);
                command.H(4, "Example:");
                command.Pre(_factory.GetExampleValue(p.response)).Css("code");
            }

            _html = root.ToString(SaveOptions.None);
        }

        private void BuildClassHelp(XElement body, Type classType)
        {
            var table = body.Table();
            table.Tr().Th("Name").Th("Type").Th("Description");
            foreach (var mem in classType.GetMembers().OrderBy( m => m.Name))
            {
                switch (mem)
                {
                    case PropertyInfo p:
                        {
                            var attr = (p.GetCustomAttribute( typeof(ParameterAttribute), true) as ParameterAttribute) ?? ParameterAttribute.Empty;
                            table.Tr().Td(p.Name).Td(p.PropertyType.Name).Td(attr.Description);
                        }
                        break;
                    case FieldInfo f:
                        {
                            var attr = (f.GetCustomAttribute(typeof(ParameterAttribute), true) as ParameterAttribute) ?? ParameterAttribute.Empty;
                            table.Tr().Td(f.Name).Td(f.FieldType.Name).Td(attr.Description);
                        }
                        break;
                }
            }
            
        }

        private static string GetResponseName(string requestName)
        {
            int index = requestName.LastIndexOf("Request");
            string strippedName = requestName.Substring(0, index);
            return $"{strippedName}Response";
        }

        public string HTML
        {
            get { return _html; }
        }
    }

    static class HtmlExtensions
    {
        private static XElement AddChild(XElement parent, string name, object content = null)
        {
            XElement child = new XElement(name, content);
            if ( null != parent )
                parent.Add(child);
            return child;
        }

        public static XElement Table(this XElement parent)
        {
            return AddChild(parent, "table");
        }

        public static XElement Tr(this XElement parent)
        {
            return AddChild(parent, "tr");
        }

        public static XElement Td(this XElement parent, object content)
        {
            AddChild(parent, "td", content);
            return parent;
        }

        public static XElement Th(this XElement parent, object content)
        {
            return AddChild(parent, "th", content);
        }

        public static XElement Div(this XElement parent, object content = null)
        {
            return AddChild(parent, "div", content);
        }

        public static XElement Style(this XElement target, string style)
        {
            target.SetAttributeValue("style", style);
            return target;
        }

        public static XElement Content(this XElement target, object content)
        {
            target.SetValue(content);
            return target;
        }

        public static XElement Css(this XElement target, string cssClass)
        {
            target.SetAttributeValue("class", cssClass);
            return target;
        }

        public static XElement H(this XElement parent, int level)
        {
            return AddChild(parent, $"H{level}");
        }

        public static XElement H(this XElement parent, int level, object content)
        {
            return AddChild(parent, $"H{level}", content);
        }

        public static XElement Hr(this XElement parent)
        {
            return AddChild(parent, "hr");
        }

        public static XElement IncludeStyle(this XElement parent, string css)
        {
            AddChild(parent, "style", css);
            return parent;
        }

        public static XElement Pre(this XElement parent, string content)
        {
            return AddChild(parent, "pre", content);
        }
    }
}
