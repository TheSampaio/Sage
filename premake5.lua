workspace "Sage"
    configurations { "Debug", "Release" }

    language "C++"
    cppdialect "C++20"
    architecture "x64"

    filter "configurations:Debug"
        defines "_DEBUG"
        symbols "On"
        optimize "Off"

    filter "configurations:Release"
        defines "_RELEASE"
        symbols "Off"
        optimize "Speed"

    OutputDir = "%{cfg.buildcfg}/"

    project "Sage"
    location "Compiler"
    kind "ConsoleApp"

    -- Output directories
    targetdir ("_Output/Bin/" .. OutputDir .. "%{prj.name}")
    objdir    ("_Output/Obj/" .. OutputDir .. "%{prj.name}")

    files {
        "%{prj.location}/**.h",
        "%{prj.location}/**.sg",
        "%{prj.location}/**.cpp",
    }

    filter "system:windows"
        defines "_PLATFORM_WIN32"
        systemversion "latest"
