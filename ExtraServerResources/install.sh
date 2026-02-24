#!/bin/bash
# Installs glibc 2.35 and patches Unity server binaries to use it.
# Required for Unity 6.3+ on Amazon Linux 2023 fleets.
set -e

GLIBC_VERSION="2.35"
INSTALL_PREFIX="/opt/glibc-${GLIBC_VERSION}"

# Auto-detect architecture
ARCH=$(uname -m)
if [ "$ARCH" = "x86_64" ]; then
    INTERPRETER="${INSTALL_PREFIX}/lib/ld-linux-x86-64.so.2"
    EXE_PATTERN="*.x86_64"
    MONO_ARCH="x86_64"
elif [ "$ARCH" = "aarch64" ]; then
    INTERPRETER="${INSTALL_PREFIX}/lib/ld-linux-aarch64.so.1"
    EXE_PATTERN="*.aarch64"
    MONO_ARCH="aarch64"
else
    echo "ERROR: Unsupported architecture: $ARCH"
    exit 1
fi

echo "Detected architecture: $ARCH"

# Build glibc from source
echo "Installing glibc ${GLIBC_VERSION}"

# bison is required to build glibc
sudo dnf install bison -y

wget -c "https://ftp.gnu.org/gnu/glibc/glibc-${GLIBC_VERSION}.tar.gz"

tar -zxvf "glibc-${GLIBC_VERSION}.tar.gz"
cd "glibc-${GLIBC_VERSION}"

mkdir -p glibc-build
cd glibc-build

# Prevent warning during install
sudo touch /etc/ld.so.conf

../configure --prefix="${INSTALL_PREFIX}"

make -s all
sudo make -s install

echo ""
echo "glibc ${GLIBC_VERSION} installed to ${INSTALL_PREFIX}"

# Replacing the system glibc breaks OS utilities that depend on it
# patchelf is used to specifically set the Unity files to use the new version

sudo dnf install patchelf -y

echo "Patching binaries with new glibc"

GAME_DIR="/local/game"
GLIBC_LIB="${INSTALL_PREFIX}/lib"

# Find the executable and derive the data folder name
echo "Looking for executable in $GAME_DIR"
echo "Contents of $GAME_DIR:"
ls -la "$GAME_DIR"

EXECUTABLE=$(find "$GAME_DIR" -maxdepth 1 -name "$EXE_PATTERN" -type f | head -1)
if [ -z "$EXECUTABLE" ]; then
    echo "ERROR: No $EXE_PATTERN executable found in $GAME_DIR"
    exit 1
fi
EXECUTABLE_NAME=$(basename "$EXECUTABLE" | sed "s/\.$ARCH$//")
DATA_DIR="$GAME_DIR/${EXECUTABLE_NAME}_Data"

echo "Detected executable: $EXECUTABLE"
echo "Data directory: $DATA_DIR"

# --set-interpreter: Specifies the dynamic linker (ld.so) used to load the executable
# --set-rpath: Specifies the runtime library search path for shared object dependencies

# Patch the game server executable
echo "Patching main executable"
sudo patchelf --set-interpreter "$INTERPRETER" "$EXECUTABLE"
sudo patchelf --set-rpath "$GLIBC_LIB:$GAME_DIR:/lib64:$DATA_DIR/MonoBleedingEdge/$MONO_ARCH" "$EXECUTABLE"

# Patch UnityPlayer.so
echo "Patching UnityPlayer.so"
sudo patchelf --set-rpath "$GLIBC_LIB:/lib64" "$GAME_DIR/UnityPlayer.so"

# Patch Mono library (only for Mono builds, not IL2CPP)
MONO_LIB="$DATA_DIR/MonoBleedingEdge/$MONO_ARCH/libmonobdwgc-2.0.so"
if [ -f "$MONO_LIB" ]; then
    echo "Patching Mono library"
    sudo patchelf --set-rpath "$GLIBC_LIB:/lib64" "$MONO_LIB"
else
    echo "Mono library not found (IL2CPP build), skipping"
fi

# Patch GameAssembly.so for IL2CPP builds
GAME_ASSEMBLY="$GAME_DIR/GameAssembly.so"
if [ -f "$GAME_ASSEMBLY" ]; then
    echo "Patching GameAssembly.so (IL2CPP)"
    sudo patchelf --set-rpath "$GLIBC_LIB:/lib64" "$GAME_ASSEMBLY"
fi

echo "Patching complete."
