using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EksCdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new EksCdkStack(app, "EksCdkStack");
            app.Synth();
        }
    }
}
