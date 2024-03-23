workspace "Sage"
    configurations { "Debug", "Release" }

    language "C++"
    cppdialect "C++20"
    architecture "x64"

    filter "configurations:Debug"
        defines "LN_DEBUG"
        symbols "On"
        optimize "Off"

    filter "configurations:Release"
        defines "LN_RELEASE"
        symbols "Off"
        optimize "Speed"

    OutputDir = "%{cfg.buildcfg}/"

    project "Compiler"
    location "Compiler"
    kind "ConsoleApp"

    -- Output directories
    targetdir ("_Output/Bin/" .. OutputDir .. "%{prj.name}")
    objdir    ("_Output/Obj/" .. OutputDir .. "%{prj.name}")

    files {
        "%{prj.location}/**.h",
        "%{prj.location}/**.cpp",
        "%{prj.location}/**.sg",
        "%{prj.location}/**.sgi",
    }

    filter "system:windows"
        defines "_PLATFORM_WINDOWS_"
        systemversion "latest"
