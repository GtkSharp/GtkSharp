snk = join_paths(meson.current_source_dir(), '..', 'gtk-sharp.snk')
assembly_name = 'glib-sharp'

configure_file(input: 'glib-sharp.dll.config.in',
        output: 'glib-sharp.dll.config',
        configuration : remap_dl_data)

if install
    configure_file(input: 'glib-sharp-3.0.pc.in',
            output: 'glib-sharp-3.0.pc',
            configuration : version_data,
            install_dir: pkg_install_dir,
            install: install)
endif

policy_data = configuration_data()
policy_data.set('ASSEMBLY_NAME', assembly_name)
policy_data.set('API_VERSION', apiversion)

policy = configure_file(input: policy_config,
        output: 'policy.config',
        configuration : policy_data)

sources = [
    'Argv.cs',
    'Bytes.cs',
    'ConnectBeforeAttribute.cs',
    'Cond.cs',
    'Date.cs',
    'DateTime.cs',
    'DefaultSignalHandlerAttribute.cs',
    'DestroyNotify.cs',
    'ExceptionManager.cs',
    'FileUtils.cs',
    'GException.cs',
    'GInterfaceAdapter.cs',
    'GInterfaceAttribute.cs',
    'GLibSynchronizationContext.cs',
    'Global.cs',
    'GString.cs',
    'GType.cs',
    'GTypeAttribute.cs',
    'Idle.cs',
    'InitiallyUnowned.cs',
    'IOChannel.cs',
    'IWrapper.cs',
    'KeyFile.cs',
    'ListBase.cs',
    'List.cs',
    'Log.cs',
    'MainContext.cs',
    'MainLoop.cs',
    'ManagedValue.cs',
    'Markup.cs',
    'Marshaller.cs',
    'MissingIntPtrCtorException.cs',
    'Mutex.cs',
    'NotifyHandler.cs',
    'Object.cs',
    'ObjectManager.cs',
    'Opaque.cs',
    'ParamSpec.cs',
    'PollFD.cs',
    'Priority.cs',
    'PropertyAttribute.cs',
    'PtrArray.cs',
    'RecMutex.cs',
    'Signal.cs',
    'SignalArgs.cs',
    'SignalAttribute.cs',
    'SignalClosure.cs',
    'SList.cs',
    'Source.cs',
    'SourceFunc.cs',
    'SourceFuncs.cs',
    'SourceDummyMarshal.cs',
    'GLibSharp.SourceFuncNative.cs',
    'GLibSharp.SourceDummyMarshalNative.cs',
    'SourceCallbackFuncs.cs',
    'Spawn.cs',
    'Thread.cs',
    'Timeout.cs',
    'TimeVal.cs',
    'TimeZone.cs',
    'ToggleRef.cs',
    'TypeFundamentals.cs',
    'TypeInitializerAttribute.cs',
    'TypeNameAttribute.cs',
    'ValueArray.cs',
    'Value.cs',
    'Variant.cs',
    'VariantType.cs' ]


glib_sharp = library(assembly_name, sources,
        cs_args: ['-unsafe', '-keyfile:' + snk],
        install: install,
        install_dir: lib_install_dir
)

install_infos += [assembly_name, glib_sharp.full_path()]

glib_api_includes = join_paths(meson.current_source_dir(), 'glib-api.xml')

if install
    install_data(glib_api_includes, install_dir: gapi_xml_installdir)
endif

glib_sharp_dep = declare_dependency(link_with: glib_sharp)
