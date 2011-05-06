﻿// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;

namespace System.ComponentModel.Composition
{
    internal enum CompositionErrorId : int
    {
        Unknown = 0,
        InvalidExportMetadata,
        RequiredMetadataNotFound,
        UnsupportedExportType,
        ImportNotSetOnPart,
        ImportEngine_ComposeTookTooManyIterations,
        ImportEngine_ImportCardinalityMismatch,
        ImportEngine_PartCycle,
        ImportEngine_PartCannotSetImport,
        ImportEngine_PartCannotGetExportedValue,
        ImportEngine_PartCannotActivate,
        ImportEngine_PreventedByExistingImport,
        ImportEngine_InvalidStateForRecomposition,
        ReflectionModel_PartConstructorMissing,
        ReflectionModel_PartConstructorThrewException,
        ReflectionModel_PartOnImportsSatisfiedThrewException,
        ReflectionModel_ExportNotReadable,
        ReflectionModel_ExportThrewException,
        ReflectionModel_ExportMethodTooManyParameters,
        ReflectionModel_ImportNotWritable,
        ReflectionModel_ImportThrewException,
        ReflectionModel_ImportNotAssignableFromExport,        
        ReflectionModel_ImportCollectionNull,
        ReflectionModel_ImportCollectionNotWritable,
        ReflectionModel_ImportCollectionConstructionThrewException,
        ReflectionModel_ImportCollectionGetThrewException,
        ReflectionModel_ImportCollectionIsReadOnlyThrewException,
        ReflectionModel_ImportCollectionClearThrewException,
        ReflectionModel_ImportCollectionAddThrewException,
        ReflectionModel_ImportManyOnParameterCanOnlyBeAssigned,
    }
}
