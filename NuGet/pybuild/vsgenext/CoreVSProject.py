#!/usr/bin/python3

import os, json
from vsgen.project import VSGProject
from xml.etree import ElementTree as et
from yattag import indent
from os.path import abspath, join
from collections import OrderedDict

class CoreVSProject(VSGProject):
    """
    CoreVSProject extends :class:`~vsgen.project.VSGProject` with data and logic needed to create a `.xproj` file.
    :ivar str   TargetFrameworkVersion:     The target framework version.
    :ivar str   BaseIntermediateOutputPath: Intermediate path for building the output.
    :ivar str   ProjectXml:                 Override the xml within the .xproj file.
    :ivar str   ProjectJson:                Override the json within the project.lock file.
    """
    __project_type__ = 'netcore'

    __writable_name__ = "Visual Studio .Net Core Project"

    __registerable_name__ = "Visual Studio C# Compiler"

    def __init__(self, **kwargs):
        """
        Constructor.
        :param kwargs:         List of arbitrary keyworded arguments to be processed as instance variable data
        """
        super(CoreVSProject, self).__init__(**kwargs)

    def _import(self, datadict):
        """
        Internal method to import instance variables data from a dictionary
        :param dict datadict: The dictionary containing variables values.
        """
        super(CoreVSProject, self)._import(datadict)
        self.TargetFrameworkVersion = datadict.get('TargetFrameworkVersion', 'v4.6.1')
        self.BaseIntermediateOutputPath = datadict.get('BaseIntermediateOutputPath', '.\obj')
        self.ProjectXml = datadict.get('ProjectXml', None)
        self.ProjectJson = datadict.get('ProjectJson', None)
        self.Version = datadict.get('Version', '1.0.0-*')
        self.Depends = datadict.get('Depends', {'NETStandard.Library': '1.6.0'})
        self.Frameworks = datadict.get('Frameworks', None)
        self.BuildOptions = datadict.get('BuildOptions', None)

    def get_project_json(self):
        """
        Get the json for use in Project.lock
        """

        data = OrderedDict()
        ver = {'version': self.Version}
        data.update(ver)
 
        depends = self.Depends
        depends2 = {'dependencies': depends}
        data.update(depends2)
        
        if self.Frameworks != None:
            frameworks = {'frameworks': self.Frameworks}
        else:
            frameworks = {'frameworks': {'netstandard1.6': {'imports': 'dnxcore50'}}}
        data.update(frameworks)

        if self.BuildOptions != None:
            buildopts = {'buildOptions': self.BuildOptions}
            data.update(buildopts)

        return data

    def get_project_xml(self):
        """
        Get the xml for use in the xproj file
        """

        xml_projroot = et.Element('Project')
        xml_projroot.set('ToolsVersion', '14.0')
        xml_projroot.set('DefaultTargets', 'Build')
        xml_projroot.set('xmlns', 'http://schemas.microsoft.com/developer/msbuild/2003')

        propgroup1 = et.SubElement(xml_projroot, 'PropertyGroup')
        studiover = et.SubElement(propgroup1, 'VisualStudioVersion')
        studiover.set('Condition', "'$(VisualStudioVersion)' == ''")
        studiover.text = '14.0'
        vstoolspath = et.SubElement(propgroup1, 'VSToolsPath')
        vstoolspath.set('Condition', "'$(VSToolsPath)' == ''")
        vstoolspath.text = r"$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)"

        import1 = et.SubElement(xml_projroot, 'Import')
        import1.set('Project', '$(VSToolsPath)\DotNet\Microsoft.DotNet.Props')
        import1.set('Condition', "'$(VSToolsPath)' != ''")

        propgroup2 = et.SubElement(xml_projroot, 'PropertyGroup')
        propgroup2.set('Label', 'Globals')
        projguid = et.SubElement(propgroup2, 'ProjectGuid')
        projguid.text = self.lower(self.GUID)
        rootnamespace = et.SubElement(propgroup2, 'RootNamespace')
        rootnamespace.text = self.RootNamespace
        baseintermediateoutputpath = et.SubElement(propgroup2, 'BaseIntermediateOutputPath')
        baseintermediateoutputpath.set('Condition', "'$(BaseIntermediateOutputPath)'=='' ")
        baseintermediateoutputpath.text = self.BaseIntermediateOutputPath
        targetframeworkversion = et.SubElement(propgroup2, 'TargetFrameworkVersion')
        targetframeworkversion.text = self.TargetFrameworkVersion

        propgroup3 = et.SubElement(xml_projroot, 'PropertyGroup')
        schemaver = et.SubElement(propgroup3, 'SchemaVersion')
        schemaver.text = '2.0'

        import2 = et.SubElement(xml_projroot, 'Import')
        import2.set('Project', '$(VSToolsPath)\DotNet\Microsoft.DotNet.targets')
        import2.set('Condition', "'$(VSToolsPath)' != ''")

        etstr = et.tostring(xml_projroot, encoding='utf-8', method='xml').decode('utf-8')
        outtxt = indent(etstr)
        return outtxt 


    def write(self):
        """
        Creates the project files.
        """
        npath = os.path.normpath(self.FileName)
        (filepath, filename) = os.path.split(npath)
        os.makedirs(filepath, exist_ok=True)

        projectFileName = os.path.normpath(self.FileName)
        projxml = ''
        if self.ProjectXml == None:
            projxml = self.get_project_xml()
        else:
            projxml = self.ProjectXml
        with open(projectFileName, 'wt') as f:
            f.write(projxml)

        jsonFileName = join(filepath, 'project.json')

        if self.ProjectJson == None:
            projjson = self.get_project_json()
        else:
            projjson = self.ProjectJson

        with open(jsonFileName, 'w') as f:
            txt = json.dumps(projjson, indent=2, separators=(',', ': '))
            f.write(txt)
