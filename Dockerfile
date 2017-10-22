FROM debian:9

RUN apt-get update && \
    apt-get install -y git mono-devel libgtk-3-dev msbuild

RUN pip3 install git+https://github.com/mesonbuild/meson/

ENV SOURCE_DIR="/source"

RUN mkdir -p "${SOURCE_DIR}"

COPY / "${SOURCE_DIR}"

CMD ["/source/.ci_build.sh"]
