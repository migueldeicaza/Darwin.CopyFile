all: doc-update yaml

doc-update:
	dotnet build --configuration release
	mdoc update -i ./bin/Release/netstandard1.5/CopyFile.xml -o ecmadocs/en ./bin/Release/netstandard1.5/CopyFile.dll
	perl -pi -e 's/To be added.//' ecmadocs/en/Darwin/*xml

yaml:
	-rm ecmadocs/en/ns-.xml
	mono /cvs/ECMA2Yaml/ECMA2Yaml/ECMA2Yaml/bin/Debug/ECMA2Yaml.exe --source=`pwd`/ecmadocs/en --output=`pwd`/docfx/api
	(cd docfx; mono ~/Downloads/docfx/docfx.exe build)
