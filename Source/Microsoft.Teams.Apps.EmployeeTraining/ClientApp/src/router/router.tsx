/*
    <copyright file="router.tsx" company="Microsoft">
    Copyright (c) Microsoft. All rights reserved.
    </copyright>
*/

import * as React from "react";
import { BrowserRouter, Route, Switch } from "react-router-dom";
import ErrorPage from '../components/error-page';
import "../i18n";
import RegisterRemoveEvent from "../components/register-remove-event/register-remove-event";
import DiscoverEvents from "../components/discover-events/discover-events";
import MyEvents from "../components/my-events/my-events";
import MobileFilter from "../components/mobile-filter-bar/mobile-filter-bar";
import ManageEvents from "../components/manage-events/manage-events";
import CreateEventWrapper from "../components/create-event/create-event-wrapper";
import ManageCategories from "../components/manage-categories/manage-categories";
import CloseOrCancelEvent from "../components/manage-events/close-cancel-event";
import DeleteDraftEvent from "../components/manage-events/delete-draft";
import SignInPage from "../components/signin/signin";
import SignInSimpleStart from "../components/signin/signin-start";
import SignInSimpleEnd from "../components/signin/signin-end";

export const AppRoute: React.FunctionComponent<{}> = () => {
    return (
        <React.Suspense fallback={<div className="container-div"><div className="container-subdiv"></div></div>}>
            <BrowserRouter>
                <Switch>
                    <Route exact path="/discover-events" component={DiscoverEvents} />
                    <Route exact path="/my-events" component={MyEvents} />
                    <Route exact path="/manage-events" component={ManageEvents} />
                    <Route exact path="/manage-categories" component={ManageCategories} />
                    <Route exact path="/close-or-cancel-event" component={CloseOrCancelEvent} />
                    <Route exact path="/register-remove" component={RegisterRemoveEvent} />
                    <Route exact path="/create-event" component={CreateEventWrapper} />
                    <Route exact path="/delete-draft" component={DeleteDraftEvent} />
                    <Route exact path="/mobile-filter" component={MobileFilter} />
                    <Route exact path="/signin" component={SignInPage} />
                    <Route exact path="/signin-simple-start" component={SignInSimpleStart} />
                    <Route exact path="/signin-simple-end" component={SignInSimpleEnd} />
                    <Route exact path="/error" component={ErrorPage} />
                </Switch>
            </BrowserRouter>
        </React.Suspense>
    );
};