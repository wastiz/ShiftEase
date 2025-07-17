import {configureStore} from '@reduxjs/toolkit'
import { persistStore, persistReducer } from 'redux-persist'
import storage from 'redux-persist/lib/storage'
import { commonReducer } from './slices/commonSlice'

const persistConfig = {
    key: 'common',
    storage,
}

const persistedCommonReducer = persistReducer(persistConfig, commonReducer)

const store = configureStore({
    reducer: {
        common: persistedCommonReducer,
    },
		middleware: getDefaultMiddleware => getDefaultMiddleware(),
		devTools: process.env.NODE_ENV !== 'production'
})

const persistor = persistStore(store)

export { store, persistor }