// <copyright file="color-picker.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import React from 'react';
import { SwatchColorPicker } from '@fluentui/react/lib';
import Resources from '../../../constants/resources';
import "./color-picker.css";

interface IColorPickerProps {
    onColorChange: (id?: string, color?: string) => void,
    selectedColor?: string,
}

export const ColorPicker: React.FunctionComponent<IColorPickerProps> = props => {
    const [selectedId, setSelectedId] = React.useState<string | undefined>();
    const [divKey, setDivKey] = React.useState<number>(1);

    React.useEffect(() => {
        var selectedIdFound = Resources.colorCells.find(x => x.color === props.selectedColor);
        setSelectedId(selectedIdFound?.id);
        setDivKey(divKey! + 1);
    }, [props.selectedColor]);

    return (
        <div className="color-picker" key={divKey}>
            <SwatchColorPicker columnCount={5} cellShape={'circle'} colorCells={Resources.colorCells} selectedId={selectedId} onColorChanged={props.onColorChange }/>
        </div>
    );
}