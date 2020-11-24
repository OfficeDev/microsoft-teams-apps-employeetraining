// <copyright file="no-content-page.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from 'react';
import { Text } from "@fluentui/react-northstar";
import { EyeIcon } from "@fluentui/react-icons-northstar";

interface INoContentPage {
    message: string
}

const NoContent: React.FunctionComponent<INoContentPage> = props => {
    return (
        <div className="no-content-container">
            <div className="app-logo">
                <EyeIcon size="largest" />
            </div>
            <div className="no-content-title">
                <Text content={props.message} weight="semibold" />
            </div>
        </div>
    );
}

export default NoContent;