#-------------------------------------------------
#
# Project created by QtCreator 2017-05-31T14:08:15
#
#-------------------------------------------------

CONFIG += c++11
CONFIG += plugin

TARGET = QmlNet
TEMPLATE = lib

DEFINES += QMLNET_LIBRARY
DEFINES += QT_DEPRECATED_WARNINGS

include(QmlNet.pri)

target.path = $$(PREFIX)/
INSTALLS += target

CONFIG(install-qt-libs) {
    win32 {
        qtlibs.path = $$(PREFIX)
        qtlibs.files = $$[QT_INSTALL_BINS]/*.dll
        INSTALLS += qtlibs

        qtplugins.path = $$(PREFIX)/plugins
        qtplugins.files = $$[QT_INSTALL_PLUGINS]/*
        INSTALLS += qtplugins

        qtqml.path = $$(PREFIX)/qml
        qtqml.files = $$[QT_INSTALL_QML]/*
        INSTALLS += qtqml
    }
    macx {
        qtlibs.path = $$(PREFIX)/lib
        qtlibs.files = $$[QT_INSTALL_LIBS]/*
        INSTALLS += qtlibs

        qtplugins.path = $$(PREFIX)/plugins
        qtplugins.files = $$[QT_INSTALL_PLUGINS]/*
        INSTALLS += qtplugins

        qtqml.path = $$(PREFIX)/qml
        qtqml.files = $$[QT_INSTALL_QML]/*
        INSTALLS += qtqml

        QMAKE_RPATHDIR += @loader_path/lib
    }
    unix:!macx {
        qtlibs.path = $$(PREFIX)/lib
        qtlibs.files = $$[QT_INSTALL_LIBS]/*
        INSTALLS += qtlibs

        qtplugins.path = $$(PREFIX)/plugins
        qtplugins.files = $$[QT_INSTALL_PLUGINS]/*
        INSTALLS += qtplugins

        qtqml.path = $$(PREFIX)/qml
        qtqml.files = $$[QT_INSTALL_QML]/*
        INSTALLS += qtqml

        QMAKE_RPATHDIR = $ORIGIN/lib
    }
}
