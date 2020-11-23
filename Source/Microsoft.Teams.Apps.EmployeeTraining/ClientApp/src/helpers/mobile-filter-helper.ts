// <copyright file="mobile-filter-helper.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import Resources from "../constants/resources";

/** Clears the local storage data saved for mobile filter */
export const clearMobileFilterLocalStorage = () => {
    localStorage.removeItem(Resources.userEventsMobileFilteredCategoriesLocalStorageKey);
    localStorage.removeItem(Resources.userEventsMobileFilteredUsersLocalStorageKey);
    localStorage.removeItem(Resources.userEventsMobileSortByFilterLocalStorageKey);
}