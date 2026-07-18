param(
    [string]$JsonPath,
    [string]$OutputPath,
    [string]$ProjectDir
)

try {
    Write-Host "Generating translation key constants from JSON: $JsonPath" -ForegroundColor Green
    
    if (-not (Test-Path $JsonPath)) {
        Write-Warning "JSON file not found: $JsonPath"
        exit 0
    }
    
    $jsonContent = [System.IO.File]::ReadAllText($JsonPath, [System.Text.Encoding]::UTF8)
    
    if ([string]::IsNullOrEmpty($jsonContent)) {
        Write-Warning "JSON file is empty: $JsonPath"
        exit 0
    }
    
    # Extract keys and their values using regex
    $keys = [System.Collections.Generic.List[string]]::new()
    $keyValues = @{}
    $excludedKeys = @("LanguageID")
    
    # Pattern to match both key and value
    $pattern = '"([^"]+)"\s*:\s*"([^"]*)"'
    $matches = [System.Text.RegularExpressions.Regex]::Matches($jsonContent, $pattern)
    
    foreach ($match in $matches) {
        $key = $match.Groups[1].Value
        $value = $match.Groups[2].Value
        
        if ($excludedKeys -contains $key) {
            continue
        }
        
        if (-not $keys.Contains($key)) {
            $keys.Add($key)
            $keyValues[$key] = $value
        }
    }
    
    if ($keys.Count -eq 0) {
        Write-Warning "No keys found in JSON: $JsonPath"
        exit 0
    }
    
    Write-Host "Found $($keys.Count) keys in JSON" -ForegroundColor Gray
    
    # Get the class name from the output file name (without extension)
    $className = [System.IO.Path]::GetFileNameWithoutExtension($OutputPath)
    
    # If class name is empty, use a default
    if ([string]::IsNullOrEmpty($className)) {
        $className = "TranslationKeys"
    }
    
    # Get the project name for the summary
    $projectName = if ($ProjectDir) {
        Split-Path $ProjectDir -Leaf
    } else {
        "BetterAmongUs"
    }
    
    # Generate the class content
    $classLines = @(
        "// auto-generated",
        "using BetterAmongUs.Modules;",
        "",
        "namespace BetterAmongUs.Generated;",
        "",
        "/// <summary>",
        "/// Provides strongly-typed translation keys for BAU.",
        "/// Each constant represents a translation key that can be used with the Translator.",
        "/// </summary>",
        "public static class $className",
        "{",
        "    /// <summary>",
        "    /// Represents a translation key with the ability to get the localized string.",
        "    /// </summary>",
        "    public readonly struct TranslationString(string key)",
        "    {",
        "        public readonly string Key = key;",
        "        public string LocalizedString => Translator.GetString(this);",
        "        public override string ToString() => LocalizedString;",
        "        public string Format(params string[] strings)",
        "        {",
        "            return string.Format(LocalizedString, args: strings);",
        "        }",
        "        public string Format(params TranslationString[] translationStrings)",
        "        {",
        "            var stringArgs = translationStrings.Select(ts => ts.LocalizedString).ToArray();",
        "            return string.Format(LocalizedString, stringArgs);",
        "        }",
        "        public string Format(params object[] args)",
        "        {",
        "            var stringArgs = args.Select(arg => arg.ToString()).ToArray();",
        "            return string.Format(LocalizedString, stringArgs);",
        "        }",
        "    }"
    )
    
    # Generate constant fields
    foreach ($key in $keys) {
        $cleanKey = $key
        
        # Clean the key for C# identifier
        if ($cleanKey -match '^[0-9]') {
            $cleanKey = "_" + $cleanKey
        }
        $cleanKey = $cleanKey -replace '[^a-zA-Z0-9_]', '_'
        $cleanKey = $cleanKey -replace '__+', '_'
        $cleanKey = $cleanKey -replace '_$', ''
        
        # Escape the key string
        $escapedKey = $key -replace '\\', '\\\\'
        $escapedKey = $escapedKey -replace '"', '\"'
        $escapedKey = $escapedKey -replace "`n", '\n'
        $escapedKey = $escapedKey -replace "`r", '\r'
        $escapedKey = $escapedKey -replace "`t", '\t'
        
        # Get the translation value and escape it for XML comment
        $translationValue = $keyValues[$key]
        if ($translationValue -ne $null) {
            # Escape XML special characters
            $translationValue = $translationValue -replace '&', '&amp;'
            $translationValue = $translationValue -replace '<', '&lt;'
            $translationValue = $translationValue -replace '>', '&gt;'
            $translationValue = $translationValue -replace '"', '&quot;'
            $translationValue = $translationValue -replace "'", '&apos;'
        }
        
        # Add summary with translation value
        $classLines += ""
        $classLines += "    /// <summary>"
        $classLines += "    /// Base Translation: $translationValue"
        $classLines += "    /// </summary>"
        $classLines += "    public static readonly TranslationString $cleanKey = new(`"$escapedKey`");"
    }
    
    $classLines += "}"
    
    $outputDir = Split-Path $OutputPath -Parent
    if (-not (Test-Path $outputDir)) {
        New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
    }
    
    $classContent = $classLines -join "`r`n"
    [System.IO.File]::WriteAllText($OutputPath, $classContent, [System.Text.UTF8Encoding]::new($false))
    
    Write-Host "Successfully generated translation key constants from JSON to: $OutputPath" -ForegroundColor Green
    Write-Host "Generated class: $className" -ForegroundColor Gray
    exit 0
}
catch {
    Write-Host "Failed to generate translation key constants from JSON: $_" -ForegroundColor Red
    Write-Host "Error details: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack trace: $($_.ScriptStackTrace)" -ForegroundColor Red
    exit 1
}